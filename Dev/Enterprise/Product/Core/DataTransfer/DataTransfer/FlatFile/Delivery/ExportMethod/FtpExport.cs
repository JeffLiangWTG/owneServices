using System;
using System.IO;
using System.Net;
using System.Threading;
using CargoWise.Common;
using CargoWise.ComponentModel;
using CargoWise.IO;
using CargoWise.Types;
using Enterprise.ZArchitecture;
using Enterprise.ZArchitecture.Environment;

namespace Enterprise.DataTransfer.Business
{
	public class FtpExport : FileExport
	{
		public FtpExport(ExportInstructions instructions, INotifications notifications)
			: base(instructions, notifications)
		{
		}

		public override ExportType ExportType
		{
			get
			{
				return ExportType.Ftp;
			}
		}

		public override void Deliver(ZString savedExportFile)
		{
			try
			{
				base.Deliver(savedExportFile);
				if (IsReadyToFtp)
				{
					DeliverViaFtp();
				}
			}
			finally
			{
				if (File.Exists(Instructions.OutputFile))
				{
					File.Delete(Instructions.OutputFile);
				}

				if (File.Exists(savedExportFile))
				{
					File.Delete(savedExportFile);
				}
			}
		}

		public override bool CanDeliver
		{
			get
			{
				Instructions.FtpProperties.RunPreSaveValidation();
				return !Instructions.FtpProperties.HasErrors;
			}
		}

		void DeliverViaFtp()
		{
			try
			{
				FtpState state = new FtpState();
				state.FileName = Instructions.OutputFile;

				Uri target = new Uri(Instructions.FtpProperties.UriString + "/" + Path.GetFileName(state.FileName + ".lck"));
				FtpWebRequest request = GetFtpWebRequest(target);
				request.Method = WebRequestMethods.Ftp.UploadFile;

				state.Request = request;
#if DEBUG
				CallBeginUploadMock(state);
#else
				BeginUpload(state);
#endif
				if (state.OperationException != null)
				{
					throw state.OperationException;
				}
				else
				{
					Notifications.Notify(new InfoNotification(Res.GetString("5c854e9e-4108-46a3-b279-4de88a7a3106", "Data FTP Completed - {0}", state.StatusDescription)));
				}
			}
			catch (Exception e) when (!e.IsCriticalException())
			{
				string error = Res.GetString("fca010c3-9fd9-4f93-b01c-f73c2e3464df", "There was an error uploading the file {0}: {1}", Instructions.OutputFile, e.Message);
				Notifications.Notify(new ErrorNotification(ErrorType.Error, error));
			}
		}

		protected FtpWebRequest GetFtpWebRequest(Uri target)
		{
#pragma warning disable SYSLIB0014 // WebClient.WebClient()' is obsolete: 'WebRequest, HttpWebRequest, ServicePoint, and WebClient are obsolete. Use HttpClient instead.'
			FtpWebRequest result = (FtpWebRequest)WebRequest.Create(target);
#pragma warning restore SYSLIB0014
			result.Credentials = new NetworkCredential(Instructions.FtpProperties.Username, Instructions.FtpProperties.Password);
			result.Proxy = Instructions.FtpProperties.Proxy;
			result.UseBinary = true;
			return result;
		}

#if DEBUG
		protected virtual void CallBeginUploadMock(FtpState state)
		{
			if (Globals.IsTest)
			{
				UnitTestFtp.Instance.SetFtpResult(state);
			}
			else
			{
				BeginUpload(state);
			}

			state.OperationComplete.Set();
		}
#endif

		protected void BeginUpload(FtpState state)
		{
			ManualResetEvent waitObject = state.OperationComplete;
			state.Request.BeginGetRequestStream(new AsyncCallback(EndGetStreamCallback), state);
			waitObject.WaitOne();
			if (state.OperationException == null)
			{
				waitObject.Reset();
				state.Request = GetFtpWebRequest(state.Request.RequestUri);
				state.Request.RenameTo = Path.GetFileName(state.FileName);
				state.Request.Method = WebRequestMethods.Ftp.Rename;
				state.Request.BeginGetResponse(new AsyncCallback(EndGetResponseCallback), state);
				waitObject.WaitOne();
			}
		}

		static void EndGetStreamCallback(IAsyncResult asyncResult)
		{
			FtpState state = (FtpState)asyncResult.AsyncState;

			Stream requestStream = null;
			try
			{
				requestStream = state.Request.EndGetRequestStream(asyncResult);
				const int BufferLength = 2048;
				byte[] buffer = new byte[BufferLength];
				int count = 0;
				int readBytes = 0;
				using (FileStream stream = File.OpenRead(state.FileName))
				{
					do
					{
						readBytes = stream.Read(buffer, 0, BufferLength);
						requestStream.Write(buffer, 0, readBytes);
						count += readBytes;
					}
					while (readBytes != 0);
				}
				// IMPORTANT: Close the request stream before sending the request.
				requestStream.Close();
				state.Request.BeginGetResponse(
					new AsyncCallback(EndGetResponseCallback),
					state
				);
			}
			catch (Exception e) when (!e.IsCriticalException())
			{
				state.OperationException = e;
				state.OperationComplete.Set();
			}
		}

		static void EndGetResponseCallback(IAsyncResult asyncResult)
		{
			FtpState state = (FtpState)asyncResult.AsyncState;
			FtpWebResponse response = null;
			try
			{
				response = (FtpWebResponse)state.Request.EndGetResponse(asyncResult);
				response.Close();
				state.StatusDescription = response.StatusDescription;
				state.OperationComplete.Set();
			}
			catch (Exception e) when (!e.IsCriticalException())
			{
				state.OperationException = e;
				state.OperationComplete.Set();
			}
		}

		bool IsReadyToFtp
		{
			get
			{
				return IsOutputFileExist;
			}
		}

		bool IsOutputFileExist
		{
			get
			{
				return File.Exists(Instructions.OutputFile);
			}
		}

		protected override bool IsRemoteFile
		{
			get { return false; }
		}
	}

	public class FtpState
	{
		public FtpState()
		{
			Wait = new ManualResetEvent(false);
		}

		public ManualResetEvent OperationComplete
		{
			get
			{
				return Wait;
			}
		}
		readonly ManualResetEvent Wait;

		public FtpWebRequest Request
		{
			get
			{
				return fRequest;
			}
			set
			{
				fRequest = value;
			}
		}
		FtpWebRequest fRequest;

		public string FileName
		{
			get
			{
				return fFileName;
			}
			set
			{
				fFileName = value;
			}
		}
		string fFileName;

		public Exception OperationException
		{
			get
			{
				return fOperationException;
			}
			set
			{
				fOperationException = value;
			}
		}
		Exception fOperationException;

		public string StatusDescription
		{
			get
			{
				return fStatusDescription;
			}
			set
			{
				fStatusDescription = value;
			}
		}
		string fStatusDescription;
	}

#if DEBUG
	public class UnitTestFtp
	{
		protected UnitTestFtp()
		{
		}

		public static UnitTestFtp Instance
		{
			get
			{
				if (fInstance == null)
				{
					fInstance = new UnitTestFtp();
				}
				return fInstance;
			}
		}

		[ThreadStatic] static UnitTestFtp fInstance;

		public void ResetFtpNotTransferBecauseInTestMode()
		{
			if (Directory.Exists(fDummyFTPDirectory))
			{
				Directory.Delete(fDummyFTPDirectory, true);
			}
			LastRequestUri = "";
			LastFilename = "";
			OperationExceptionForTest = null;
			StatusDescriptionForTest = "";
		}

		public void SetFtpResult(FtpState state)
		{
			state.OperationException = OperationExceptionForTest;
			state.StatusDescription = StatusDescriptionForTest;
			LastRequestUri = state.Request.RequestUri.OriginalString;
			LastFilename = state.FileName;
			if (OperationExceptionForTest == null)
			{
				string ftpFilename = Path.Combine(DummyFTPDirectory, Path.GetFileName(LastFilename));
				File.Copy(LastFilename, ftpFilename, true);
				File.SetAttributes(ftpFilename, FileAttributes.Normal);
			}
		}

		public string DummyFTPDirectory
		{
			get
			{
				if (!Directory.Exists(fDummyFTPDirectory))
				{
					fDummyFTPDirectory = Path.Combine(Temp.TempPath, "DummyFTPDirectory");
					Directory.CreateDirectory(fDummyFTPDirectory);
				}
				return fDummyFTPDirectory;
			}
		}
		string fDummyFTPDirectory;

		public string LastRequestUri = "";
		public string LastFilename = "";
		public Exception OperationExceptionForTest;
		public string StatusDescriptionForTest = "";
	}
#endif
}
