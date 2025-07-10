using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.IO;
using System.Net;
using System.Net.Security;
using System.Net.Sockets;
using System.Security.Cryptography.X509Certificates;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace CargoWise.IO.Testing.SharpFtpServer
{
	[SuppressMessage("CargoWiseOne", "CW1060:DoNotUseDateTimeNow", Justification = "Testing")]
	public class ClientConnection : IDisposable
	{
		bool CommandIsWaitingForDataStreamFlush;
		readonly List<Task> DataConnectionTasks;

		class DataConnectionOperation
		{
			public Func<Stream, string, string> Operation { get; set; }
			public string Arguments { get; set; }
		}

		#region Copy Stream Implementations

		static long CopyStream(Stream input, Stream output, int bufferSize)
		{
			byte[] buffer = new byte[bufferSize];
			int count = 0;
			long total = 0;

			while ((count = input.Read(buffer, 0, buffer.Length)) > 0)
			{
				output.Write(buffer, 0, count);
				total += count;
			}

			return total;
		}

		static long CopyStreamAscii(Stream input, Stream output, int bufferSize)
		{
			char[] buffer = new char[bufferSize];
			int count = 0;
			long total = 0;

			using (StreamReader rdr = new StreamReader(input, Encoding.ASCII))
			{
				using (StreamWriter wtr = new StreamWriter(output, Encoding.ASCII))
				{
					while ((count = rdr.Read(buffer, 0, buffer.Length)) > 0)
					{
						wtr.Write(buffer, 0, count);
						total += count;
					}
				}
			}

			return total;
		}

		long CopyStream(Stream input, Stream output)
		{
			Stream limitedStream = output; // new RateLimitingStream(output, 131072, 0.5);

			if (_connectionType == TransferType.Image)
			{
				return CopyStream(input, limitedStream, 4096);
			}

			return CopyStreamAscii(input, limitedStream, 4096);
		}

		#endregion

		#region Enums

		enum TransferType
		{
			Ascii,
			Image
		}

		enum DataConnectionType
		{
			Passive,
			Active,
		}

		enum ProtType
		{
			Clear,
			Safe,
			Confidential,
			Private,
		}
		#endregion

		TcpListener _passiveListener;

		readonly TcpClient _controlClient;
		TcpClient _dataClient;

		NetworkStream _controlStream;
		StreamReader _controlReader;
		StreamWriter _controlWriter;

		TransferType _connectionType = TransferType.Ascii;
		DataConnectionType _dataConnectionType = DataConnectionType.Active;

		string _username;
		string _root;
		string _currentDirectory;
		IPEndPoint _dataEndpoint;
		IPEndPoint _remoteEndPoint;

		X509Certificate _cert;
		SslStream _sslStream;
		ProtType _prot = ProtType.Clear;

		string _clientIP;

		User _currentUser;

		readonly List<string> _validCommands;
		readonly UserStore _userStore;

		public ClientConnection(TcpClient client, UserStore userStore)
		{
			_controlClient = client;

			_validCommands = new List<string>();

			DataConnectionTasks = new List<Task>();
			_userStore = userStore;
		}

		string CheckUser()
		{
			if (_currentUser == null)
			{
				return "530 Not logged in";
			}

			return null;
		}

		[SuppressMessage("Microsoft.Maintainability", "CA1502:AvoidExcessiveComplexity", Justification = "BaseLine")]
		public void HandleClient(object obj)
		{
			_remoteEndPoint = (IPEndPoint)_controlClient.Client.RemoteEndPoint;

			_clientIP = _remoteEndPoint.Address.ToString();

			_controlStream = _controlClient.GetStream();

			_controlReader = new StreamReader(_controlStream);
			_controlWriter = new StreamWriter(_controlStream);

			_controlWriter.WriteLine("220 Service Ready.");
			_controlWriter.Flush();

			_validCommands.AddRange(new string[] { "AUTH", "USER", "PASS", "QUIT", "HELP", "NOOP" });

			string line;

			_dataClient = new TcpClient();

			string renameFrom = null;

			try
			{
				while ((line = _controlReader.ReadLine()) != null)
				{
					string response = null;

					string[] command = line.Split(' ');

					string cmd = command[0].ToUpperInvariant();
					string arguments = command.Length > 1 ? line.Substring(command[0].Length + 1) : null;

					if (arguments != null && arguments.Trim().Length == 0)
					{
						arguments = null;
					}

					LogEntry logEntry = new LogEntry
					{
						Date = DateTime.Now,
						CIP = _clientIP,
						CSUriStem = arguments
					};

					if (!_validCommands.Contains(cmd))
					{
						response = CheckUser();
					}

					if (cmd != "RNTO")
					{
						renameFrom = null;
					}

					CommandIsWaitingForDataStreamFlush = true;

					if (response == null)
					{
						switch (cmd)
						{
							case "USER":
								response = User(arguments);
								break;
							case "PASS":
								response = Password(arguments);
								logEntry.CSUriStem = "******";
								break;
							case "CWD":
								response = ChangeWorkingDirectory(arguments);
								break;
							case "CDUP":
								response = ChangeWorkingDirectory("..");
								break;
							case "QUIT":
								response = "221 Service closing control connection";
								break;
							case "REIN":
								_currentUser = null;
								_username = null;
								_passiveListener = null;
								_dataClient = null;
								response = "220 Service ready for new user";
								break;
							case "PORT":
								response = Port(arguments);
								logEntry.CPort = _dataEndpoint.Port.ToString();
								break;
							case "PASV":
								response = Passive();
								logEntry.SPort = ((IPEndPoint)_passiveListener.LocalEndpoint).Port.ToString();
								break;
							case "TYPE":
								response = Type(command[1], command.Length == 3 ? command[2] : null);
								logEntry.CSUriStem = command[1];
								break;
							case "STRU":
								response = Structure(arguments);
								break;
							case "MODE":
								response = Mode(arguments);
								break;
							case "RNFR":
								renameFrom = arguments;
								response = "350 Requested file action pending further information";
								break;
							case "RNTO":
								response = Rename(renameFrom, arguments);
								break;
							case "DELE":
								response = Delete(arguments);
								break;
							case "RMD":
								response = RemoveDir(arguments);
								break;
							case "MKD":
								response = CreateDir(arguments);
								break;
							case "PWD":
								response = PrintWorkingDirectory();
								break;
							case "RETR":
								response = Retrieve(arguments);
								logEntry.Date = DateTime.Now;
								break;
							case "STOR":
								response = Store(arguments);
								logEntry.Date = DateTime.Now;
								break;
							case "STOU":
								response = StoreUnique();
								logEntry.Date = DateTime.Now;
								break;
							case "APPE":
								response = Append(arguments);
								logEntry.Date = DateTime.Now;
								break;
							case "LIST":
								response = List(arguments ?? _currentDirectory);
								logEntry.Date = DateTime.Now;
								break;
							case "SYST":
								response = "215 UNIX Type: L8";
								break;
							case "NOOP":
								response = "200 OK";
								break;
							case "ACCT":
								response = "200 OK";
								break;
							case "ALLO":
								response = "200 OK";
								break;
							case "NLST":
								response = NList(arguments ?? _currentDirectory);
								logEntry.Date = DateTime.Now;
								break;
							case "SITE":
								response = "502 Command not implemented";
								break;
							case "STAT":
								response = "502 Command not implemented";
								break;
							case "HELP":
								response = "502 Command not implemented";
								break;
							case "SMNT":
								response = "502 Command not implemented";
								break;
							case "REST":
								response = "502 Command not implemented";
								break;
							case "ABOR":
								response = "502 Command not implemented";
								break;

							// Extensions defined by rfc 2228
							case "AUTH":
								response = Auth(arguments);
								break;
							case "PBSZ":
								response = Pbsz(arguments);
								break;
							case "PROT":
								response = Prot(arguments);
								break;

							// Extensions defined by rfc 2389
							case "FEAT":
								response = FeatureList();
								break;
							case "OPTS":
								response = Options(arguments);
								break;

							// Extensions defined by rfc 3659
							case "MDTM":
								response = FileModificationTime(arguments);
								break;
							case "SIZE":
								response = FileSize(arguments);
								break;

							// Extensions defined by rfc 2428
							case "EPRT":
								response = EPort(arguments);
								logEntry.CPort = _dataEndpoint.Port.ToString();
								break;
							case "EPSV":
								response = EPassive();
								logEntry.SPort = ((IPEndPoint)_passiveListener.LocalEndpoint).Port.ToString();
								break;

							default:
								response = "502 Command not implemented";
								break;
						}
					}

					logEntry.CSMethod = cmd;
					logEntry.CSUsername = _username;
					logEntry.SCStatus = response.Substring(0, response.IndexOf(' '));

					//_log.Info(logEntry);

					if (_controlClient == null || !_controlClient.Connected)
					{
						CommandIsWaitingForDataStreamFlush = false;
						break;
					}

					_controlWriter.WriteLine(response);
					_controlWriter.Flush();

					CommandIsWaitingForDataStreamFlush = false;

					if (response.StartsWith("221"))
					{
						break;
					}

					if (cmd == "AUTH")
					{
						_cert = CreateTestCertificate();

						_sslStream = new SslStream(_controlStream);

						_sslStream.AuthenticateAsServer(_cert);

						_controlReader = new StreamReader(_sslStream);
						_controlWriter = new StreamWriter(_sslStream);
					}
				}
			}
			catch
			{
				//_log.Error(ex.Message);
			}

			Dispose();
		}

		X509Certificate CreateTestCertificate()
		{
			var certificateString = "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";
			var privateKeyBytes = Convert.FromBase64String(certificateString);
			var pfxPassword = "Password";
			return new X509Certificate2(privateKeyBytes, pfxPassword);
		}

		bool IsPathValid(string path)
		{
			return path.StartsWith(_root);
		}

		string NormalizeFilename(string path)
		{
			if (path == null)
			{
				path = string.Empty;
			}

			if (path == "/")
			{
				return _root;
			}

			if (path.StartsWith("/"))
			{
				path = new FileInfo(Path.Combine(_root, path.Substring(1))).FullName;
			}
			else
			{
				path = new FileInfo(Path.Combine(_currentDirectory, path)).FullName;
			}

			return IsPathValid(path) ? path : null;
		}

		#region FTP Commands

		string FeatureList()
		{
			_controlWriter.WriteLine("211- Extensions supported:");
			_controlWriter.WriteLine(" MDTM");
			_controlWriter.WriteLine(" SIZE");
			return "211 End";
		}

		string Options(string arguments)
		{
			return "200 Looks good to me...";
		}

		string Auth(string authMode)
		{
			if (authMode == "TLS")
			{
				return "234 Enabling TLS Connection";
			}
			else
			{
				return "504 Unrecognized AUTH mode";
			}
		}

		string User(string username)
		{
			_username = username;

			return "331 Username ok, need password";
		}

		string Password(string password)
		{
			_currentUser = _userStore.Validate(_username, password);

			if (_currentUser != null)
			{
				_root = _currentUser.HomeDir;
				_currentDirectory = _root;

				return "230 User logged in";
			}
			else
			{
				return "530 Not logged in";
			}
		}

		string ChangeWorkingDirectory(string pathname)
		{
			if (pathname == "/")
			{
				_currentDirectory = _root;
			}
			else
			{
				string newDir;

				if (pathname.StartsWith("/"))
				{
					pathname = pathname.Substring(1).Replace('/', '\\');
					newDir = Path.Combine(_root, pathname);
				}
				else
				{
					pathname = pathname.Replace('/', '\\');
					newDir = Path.Combine(_currentDirectory, pathname);
				}

				if (Directory.Exists(newDir))
				{
					_currentDirectory = new DirectoryInfo(newDir).FullName;

					if (!IsPathValid(_currentDirectory))
					{
						_currentDirectory = _root;
					}
				}
				else
				{
					_currentDirectory = _root;
				}
			}

			return "250 Changed to new directory";
		}

		string Port(string hostPort)
		{
			_dataConnectionType = DataConnectionType.Active;

			string[] ipAndPort = hostPort.Split(',');

			byte[] ipAddress = new byte[4];
			byte[] port = new byte[2];

			for (int i = 0; i < 4; i++)
			{
				ipAddress[i] = Convert.ToByte(ipAndPort[i]);
			}

			for (int i = 4; i < 6; i++)
			{
				port[i - 4] = Convert.ToByte(ipAndPort[i]);
			}

			if (BitConverter.IsLittleEndian)
			{
				Array.Reverse(port);
			}

			_dataEndpoint = new IPEndPoint(new IPAddress(ipAddress), BitConverter.ToInt16(port, 0));

			return "200 Data Connection Established";
		}

		string EPort(string hostPort)
		{
			_dataConnectionType = DataConnectionType.Active;

			char delimiter = hostPort[0];

			string[] rawSplit = hostPort.Split(new char[] { delimiter }, StringSplitOptions.RemoveEmptyEntries);

			char ipType = rawSplit[0][0];

			string ipAddress = rawSplit[1];
			string port = rawSplit[2];

			_dataEndpoint = new IPEndPoint(IPAddress.Parse(ipAddress), int.Parse(port));

			return "200 Data Connection Established";
		}

		string Passive()
		{
			_dataConnectionType = DataConnectionType.Passive;

			IPAddress localIp = ((IPEndPoint)_controlClient.Client.LocalEndPoint).Address;

			_passiveListener = new TcpListener(localIp, 0);
			_passiveListener.Start();

			IPEndPoint passiveListenerEndpoint = (IPEndPoint)_passiveListener.LocalEndpoint;

			byte[] address = passiveListenerEndpoint.Address.GetAddressBytes();
			short port;
			unchecked
			{
				port = (short)passiveListenerEndpoint.Port;
			}
			byte[] portArray = BitConverter.GetBytes(port);

			if (BitConverter.IsLittleEndian)
			{
				Array.Reverse(portArray);
			}

			return string.Format("227 Entering Passive Mode ({0},{1},{2},{3},{4},{5})", address[0], address[1], address[2], address[3], portArray[0], portArray[1]);
		}

		string EPassive()
		{
			_dataConnectionType = DataConnectionType.Passive;

			IPAddress localIp = ((IPEndPoint)_controlClient.Client.LocalEndPoint).Address;

			_passiveListener = new TcpListener(localIp, 0);
			_passiveListener.Start();

			IPEndPoint passiveListenerEndpoint = (IPEndPoint)_passiveListener.LocalEndpoint;

			return string.Format("229 Entering Extended Passive Mode (|||{0}|)", passiveListenerEndpoint.Port);
		}

		string Type(string typeCode, string formatControl)
		{
			switch (typeCode.ToUpperInvariant())
			{
				case "A":
					_connectionType = TransferType.Ascii;
					break;
				case "I":
					_connectionType = TransferType.Image;
					break;
				default:
					return "504 Command not implemented for that parameter";
			}

			if (!string.IsNullOrWhiteSpace(formatControl))
			{
				switch (formatControl.ToUpperInvariant())
				{
					case "N":
						//_formatControlType = FormatControlType.NonPrint;
						break;
					default:
						return "504 Command not implemented for that parameter";
				}
			}

			return $"200 Type set to {_connectionType}";
		}

		string Delete(string pathname)
		{
			pathname = NormalizeFilename(pathname);

			if (pathname != null)
			{
				if (File.Exists(pathname))
				{
					File.Delete(pathname);
				}
				else
				{
					return "550 File Not Found";
				}

				return "250 Requested file action okay, completed";
			}

			return "550 File Not Found";
		}

		string RemoveDir(string pathname)
		{
			pathname = NormalizeFilename(pathname);

			if (pathname == null)
			{
				return "550 Directory Not Found";
			}
			if (Directory.Exists(pathname))
			{
				Directory.Delete(pathname);
			}
			else
			{
				return "550 Directory Not Found";
			}

			return "250 Requested file action okay, completed";
		}

		string CreateDir(string pathname)
		{
			pathname = NormalizeFilename(pathname);

			if (pathname == null)
			{
				return "550 Directory Not Found";
			}
			if (!Directory.Exists(pathname))
			{
				Directory.CreateDirectory(pathname);
			}
			else
			{
				return "550 Directory already exists";
			}

			return "250 Requested file action okay, completed";
		}

		string FileModificationTime(string pathname)
		{
			pathname = NormalizeFilename(pathname);

			if (pathname == null)
			{
				return "550 File Not Found";
			}
			if (File.Exists(pathname))
			{
				return string.Format("213 {0}", File.GetLastWriteTime(pathname).ToString("yyyyMMddHHmmss.fff"));
			}

			return "550 File Not Found";
		}

		string FileSize(string pathname)
		{
			pathname = NormalizeFilename(pathname);

			if (pathname == null)
			{
				return "550 File Not Found";
			}
			if (File.Exists(pathname))
			{
				long length = 0;

				using (FileStream fs = File.Open(pathname, FileMode.Open, FileAccess.Read, FileShare.Read))
				{
					length = fs.Length;
				}

				return string.Format("213 {0}", length);
			}

			return "550 File Not Found";
		}

		string Retrieve(string pathname)
		{
			pathname = NormalizeFilename(pathname);

			if (pathname == null)
			{
				return "550 File Not Found";
			}
			if (File.Exists(pathname))
			{
				var state = new DataConnectionOperation { Arguments = pathname, Operation = RetrieveOperation };

				SetupDataConnectionOperation(state);

				return string.Format("150 Opening {0} mode data transfer for RETR", _dataConnectionType);
			}

			return "550 File Not Found";
		}

		string Store(string pathname)
		{
			pathname = NormalizeFilename(pathname);

			if (pathname == null)
			{
				return "450 Requested file action not taken";
			}
			var state = new DataConnectionOperation { Arguments = pathname, Operation = StoreOperation };

			SetupDataConnectionOperation(state);

			return string.Format("150 Opening {0} mode data transfer for STOR", _dataConnectionType);
		}

		string Append(string pathname)
		{
			pathname = NormalizeFilename(pathname);

			if (pathname == null)
			{
				return "450 Requested file action not taken";
			}
			var state = new DataConnectionOperation { Arguments = pathname, Operation = AppendOperation };

			SetupDataConnectionOperation(state);

			return string.Format("150 Opening {0} mode data transfer for APPE", _dataConnectionType);
		}

		string StoreUnique()
		{
			string pathname = NormalizeFilename(Guid.NewGuid().ToString());

			var state = new DataConnectionOperation { Arguments = pathname, Operation = StoreOperation };

			SetupDataConnectionOperation(state);

			return string.Format("150 Opening {0} mode data transfer for STOU", _dataConnectionType);
		}

		string PrintWorkingDirectory()
		{
			string current = _currentDirectory.Replace(_root, string.Empty).Replace('\\', '/');

			if (current.Length == 0)
			{
				current = "/";
			}

			return $"257 \"{current}\" is current directory.";
		}

		string List(string pathname)
		{
			pathname = NormalizeFilename(pathname);

			if (pathname != null)
			{
				var state = new DataConnectionOperation { Arguments = pathname, Operation = ListOperation };

				SetupDataConnectionOperation(state);

				return string.Format("150 Opening {0} mode data transfer for LIST", _dataConnectionType);
			}

			return "450 Requested file action not taken";
		}

		string NList(string pathname)
		{
			pathname = NormalizeFilename(pathname);

			if (pathname != null)
			{
				var state = new DataConnectionOperation { Arguments = pathname, Operation = NListOperation };

				SetupDataConnectionOperation(state);

				return string.Format("150 Opening {0} mode data transfer for NLST", _dataConnectionType);
			}

			return "450 Requested file action not taken";
		}

		string Structure(string structure)
		{
			switch (structure)
			{
				case "F":
					//_fileStructureType = FileStructureType.File;
					break;
				case "R":
				case "P":
					return string.Format("504 STRU not implemented for \"{0}\"", structure);
				default:
					return string.Format("501 Parameter {0} not recognized", structure);
			}

			return "200 Command OK";
		}

		string Mode(string mode)
		{
			if (mode.ToUpperInvariant() == "S")
			{
				return "200 OK";
			}

			return "504 Command not implemented for that parameter";
		}

		string Rename(string renameFrom, string renameTo)
		{
			if (string.IsNullOrWhiteSpace(renameFrom) || string.IsNullOrWhiteSpace(renameTo))
			{
				return "450 Requested file action not taken";
			}

			renameFrom = NormalizeFilename(renameFrom);
			renameTo = NormalizeFilename(renameTo);

			if (renameFrom != null && renameTo != null)
			{
				if (File.Exists(renameFrom))
				{
					File.Move(renameFrom, renameTo);
				}
				else if (Directory.Exists(renameFrom))
				{
					Directory.Move(renameFrom, renameTo);
				}
				else
				{
					return "450 Requested file action not taken";
				}

				return "250 Requested file action okay, completed";
			}

			return "450 Requested file action not taken";
		}

		string Pbsz(string arguments)
		{
			if (_cert != null)
			{
				if (int.TryParse(arguments, out _))
				{
					return "200 PBSZ=0";
				}
				return $"501 Value {arguments} not supported";
			}
			return "500 Command not recognised";
		}

		string Prot(string arguments)
		{
			if (_cert != null)
			{
				switch (arguments)
				{
					case "C":
						_prot = ProtType.Clear;
						return "200 Data channel protection level set to C";
					case "P":
						_prot = ProtType.Private;
						return "200 Data channel protection level set to P";
					default:
						return $"504 Value {arguments} not recognised";
				}
			}
			return "500 Command not recognised";
		}

		#endregion

		#region DataConnection Operations

		void HandleAsyncResult(IAsyncResult result)
		{
			if (_dataConnectionType == DataConnectionType.Active)
			{
				_dataClient.EndConnect(result);
			}
			else
			{
				_dataClient = _passiveListener.EndAcceptTcpClient(result);
			}
		}

		void SetupDataConnectionOperation(DataConnectionOperation state)
		{
			if (_dataConnectionType == DataConnectionType.Active)
			{
				_dataClient = new TcpClient(_dataEndpoint.AddressFamily);
				_dataClient.BeginConnect(_dataEndpoint.Address, _dataEndpoint.Port, DoDataConnectionOperation, state);
			}
			else
			{
				_passiveListener.BeginAcceptTcpClient(DoDataConnectionOperation, state);
			}
		}

		void DoDataConnectionOperation(IAsyncResult result)
		{
			DataConnectionTasks.Add(Task.Factory.StartNew(() => DoDataConnectionOperationCore(result)));
		}

		[SuppressMessage("Microsoft.Design", "CA1031:DoNotCatchGeneralExceptionTypes", Justification = "This is a test FTP server")]
		void DoDataConnectionOperationCore(IAsyncResult result)
		{
			string response = null;
			try
			{
				HandleAsyncResult(result);

				DataConnectionOperation op = result.AsyncState as DataConnectionOperation;

				using (NetworkStream dataStream = _dataClient.GetStream())
				{
					if (_sslStream != null && _prot == ProtType.Private)
					{
						using (var sslDataStream = new SslStream(dataStream))
						{
							sslDataStream.AuthenticateAsServer(_cert);
							response = op.Operation(sslDataStream, op.Arguments);
						}
					}
					else
					{
						response = op.Operation(dataStream, op.Arguments);
					}
				}
			}
			catch (Exception ex)
			{
				response = "451 " + ex.Message;
			}
			finally
			{
				try
				{
					_dataClient.Close();
					_dataClient = null;

					while (CommandIsWaitingForDataStreamFlush)
					{
						System.Threading.Thread.Yield();
					}

					_controlWriter.WriteLine(response);
					_controlWriter.Flush();
				}
				catch (Exception) { }
			}
		}

		string RetrieveOperation(Stream dataStream, string pathname)
		{
			long bytes = 0;

			using (FileStream fs = new FileStream(pathname, FileMode.Open, FileAccess.Read))
			{
				bytes = CopyStream(fs, dataStream);
			}

			return "226 Closing data connection, file transfer successful";
		}

		string StoreOperation(Stream dataStream, string pathname)
		{
			long bytes = 0;

			if (!Directory.Exists(Path.GetDirectoryName(pathname)))
			{
				return "553 Directory does not exist";
			}
			using (FileStream fs = new FileStream(pathname, FileMode.Create, FileAccess.Write, FileShare.None, 4096, FileOptions.SequentialScan))
			{
				bytes = CopyStream(dataStream, fs);
			}

			LogEntry logEntry = new LogEntry
			{
				Date = DateTime.Now,
				CIP = _clientIP,
				CSMethod = "STOR",
				CSUsername = _username,
				SCStatus = "226",
				CSBytes = bytes.ToString()
			};

			return "226 Closing data connection, file transfer successful";
		}

		string AppendOperation(Stream dataStream, string pathname)
		{
			long bytes = 0;

			using (FileStream fs = new FileStream(pathname, FileMode.Append, FileAccess.Write, FileShare.None, 4096, FileOptions.SequentialScan))
			{
				bytes = CopyStream(dataStream, fs);
			}

			LogEntry logEntry = new LogEntry
			{
				Date = DateTime.Now,
				CIP = _clientIP,
				CSMethod = "APPE",
				CSUsername = _username,
				SCStatus = "226",
				CSBytes = bytes.ToString()
			};

			return "226 Closing data connection, file transfer successful";
		}

		string ListOperation(Stream dataStream, string pathname)
		{
			StreamWriter dataWriter = new StreamWriter(dataStream, Encoding.ASCII);

			IEnumerable<string> directories = Directory.EnumerateDirectories(pathname);

			foreach (string dir in directories)
			{
				DirectoryInfo d = new DirectoryInfo(dir);

				string date = d.LastWriteTime < DateTime.Now - TimeSpan.FromDays(180) ?
					d.LastWriteTime.ToString("MMM dd  yyyy") :
					d.LastWriteTime.ToString("MMM dd HH:mm");

				string line = string.Format("drwxr-xr-x    2 2003     2003     {0,8} {1} {2}", "4096", date, d.Name);

				dataWriter.WriteLine(line);
				dataWriter.Flush();
			}

			IEnumerable<string> files = Directory.EnumerateFiles(pathname);

			foreach (string file in files)
			{
				FileInfo f = new FileInfo(file);

				string date = f.LastWriteTime < DateTime.Now - TimeSpan.FromDays(180) ?
					f.LastWriteTime.ToString("MMM dd  yyyy") :
					f.LastWriteTime.ToString("MMM dd HH:mm");

				string line = string.Format("-rw-r--r--    2 2003     2003     {0,8} {1} {2}", f.Length, date, f.Name);

				dataWriter.WriteLine(line);
				dataWriter.Flush();
			}

			LogEntry logEntry = new LogEntry
			{
				Date = DateTime.Now,
				CIP = _clientIP,
				CSMethod = "LIST",
				CSUsername = _username,
				SCStatus = "226"
			};

			return "226 Transfer complete";
		}

		string NListOperation(Stream dataStream, string pathname)
		{
			StreamWriter dataWriter = new StreamWriter(dataStream, Encoding.ASCII);

			IEnumerable<string> directories = Directory.EnumerateDirectories(pathname);

			foreach (string dir in directories)
			{
				DirectoryInfo d = new DirectoryInfo(dir);

				string line = d.Name;

				dataWriter.WriteLine(line);
				dataWriter.Flush();
			}

			IEnumerable<string> files = Directory.EnumerateFiles(pathname);

			foreach (string file in files)
			{
				FileInfo f = new FileInfo(file);

				string line = string.Format(f.Name);

				dataWriter.WriteLine(line);
				dataWriter.Flush();
			}

			LogEntry logEntry = new LogEntry
			{
				Date = DateTime.Now,
				CIP = _clientIP,
				CSMethod = "NLST",
				CSUsername = _username,
				SCStatus = "226"
			};

			return "226 Transfer complete";
		}

		#endregion

		#region IDisposable

		public void Dispose()
		{
			Dispose(true);
		}

		[SuppressMessage("Microsoft.Design", "CA1031:DoNotCatchGeneralExceptionTypes", Justification = "Wait all data connection tasks")]
		protected virtual void Dispose(bool disposing)
		{
			if (Interlocked.CompareExchange(ref disposed, 1, 0) == 0)
			{
				if (disposing)
				{
					if (_controlClient != null)
					{
						_controlClient.Close();
					}

					if (_dataClient != null)
					{
						_dataClient.Close();
					}

					if (_controlStream != null)
					{
						_controlStream.Close();
					}

					if (_controlReader != null)
					{
						_controlReader.Close();
					}

					if (_controlWriter != null)
					{
						_controlWriter.Close();
					}
				}
			}

			try
			{
				if (DataConnectionTasks.Count > 0)
				{
					Task.WaitAll(DataConnectionTasks.ToArray());
				}
			}
			catch (Exception) { }
		}

		int disposed;

		#endregion
	}
}
