using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Threading;
using System.Windows.Threading;
using Enterprise.RemoteDesktopServices.Server;
using Microsoft.Win32.SafeHandles;

namespace Enterprise.RemoteDesktopServices.Testing
{
	[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1106:DebugMessages", Justification = "Testing")]
	class MockWtsApi : IWtsApi
	{
		readonly string controlChars;
		readonly StreamWriter clientProcessStandardInput;

		public MockWtsApi(Process clientProcess, string controlChars, StreamWriter clientProcessStandardInput)
		{
			this.controlChars = controlChars;
			this.clientProcessStandardInput = clientProcessStandardInput;
			// Get client output asynchronously so we can abort the read thread.
			// Synchronously reading via clientProcess.StandardOutput.ReadLine()
			// blocks the calling thread so that thread.Abort() does nothing.
			clientProcess.OutputDataReceived += new DataReceivedEventHandler(clientProcess_OutputDataReceived);
		}

		public IntPtr VirtualChannelOpen(string virtualName, bool dynamic)
		{
#if NETFRAMEWORK
			return (IntPtr)5;
#else
			return 5;
#endif
		}

		internal VirtualChannelStream lastStream;
		internal int callsToVirtualChannelGetStream;

		public event EventHandler VirtualChannelStreamCreated;

		public Stream VirtualChannelGetStream(IntPtr channelHandle)
		{
			++callsToVirtualChannelGetStream;
			lastStream = new VirtualChannelStream(outputLines, clientProcessStandardInput);
			lastStream.streamhandle = new SafeFileHandle(channelHandle, true);
			VirtualChannelStreamCreated?.Invoke(this, null);
			return lastStream;
		}

		readonly Queue<string> outputLines = new Queue<string>();

		void clientProcess_OutputDataReceived(object sender, DataReceivedEventArgs e)
		{
			if (!string.IsNullOrEmpty(e.Data) && !e.Data.EndsWith(controlChars))
			{
				lock (outputLines)
				{
					outputLines.Enqueue(e.Data);
				}
			}
		}

		public void InjectRawMessagesToReadLoop(params string[] messages)
		{
			lock (outputLines)
			{
				lastStream.HandlingMessage = string.Empty;
				lastStream.IsRawOutput = true;

				foreach (var message in messages)
				{
					outputLines.Enqueue(message);
				}
			}

			EnterpriseChannel.Instance.MessageHandledEvent.WaitOne();
		}

		public void CloseFileHandle()
		{
			lastStream.streamhandle.Close();
		}

		public void MakeFileHandleInaccessible()
		{
			lastStream.WriteException = new UnauthorizedAccessException("Access to the path is denied");
		}

		public void MakeWinIOError()
		{
			VirtualChannelStream.WinIOException = new IOException("The handle is invalid.");
		}

		public void MakeWinUnknownIOError()
		{
			VirtualChannelStream.WinIOException = new IOException("Unknown IO Exception", unchecked((int)0x81234567));
		}

		internal class VirtualChannelStream : Stream
		{
			readonly StreamWriter clientProcessStandardInput;

			public VirtualChannelStream(Queue<string> outputLines, StreamWriter clientProcessStandardInput)
			{
				this.outputLines = outputLines;
				this.clientProcessStandardInput = clientProcessStandardInput;
			}

			public override void Close()
			{
				isClosed = true;
			}

			internal Exception ReadException { get; set; }
			internal Exception WriteException { get; set; }
			static internal Exception WinIOException { get; set; }
			internal bool IsRawOutput { get; set; }
			internal readonly AutoResetEvent LastChunkReceivedEvent = new AutoResetEvent(false);
			internal string HandlingMessage { get; set; }

			readonly Queue<string> outputLines;
			bool isClosed;

			public delegate int AsyncReadCaller(byte[] buffer, int offset, int count);
			public delegate void AsyncWriteCaller(byte[] buffer, int offset, int count);

			class StreamAsyncResult : IAsyncResult
			{
				internal object caller;
				internal IAsyncResult invokeResult;

				public object AsyncState
				{
					get { return invokeResult.AsyncState; }
				}

				public WaitHandle AsyncWaitHandle
				{
					get { return invokeResult.AsyncWaitHandle; }
				}

				public bool CompletedSynchronously
				{
					get { return invokeResult.CompletedSynchronously; }
				}

				public bool IsCompleted
				{
					get { return invokeResult.IsCompleted; }
				}
			}

			public override IAsyncResult BeginRead(byte[] buffer, int offset, int count, AsyncCallback callback, object state)
			{
				if (WinIOException != null)
				{
					Exception ex = WinIOException;
					WinIOException = null;
					throw ex;
				}

				Debug.WriteLine("VirtualChannelStream.BeginRead");
				StreamAsyncResult result = new StreamAsyncResult();
				var caller = new AsyncReadCaller(Read);
				result.caller = caller;
				result.invokeResult = caller.BeginInvoke(buffer, offset, count, callback, state);
				return result;
			}

			public override int EndRead(IAsyncResult asyncResult)
			{
				Debug.WriteLine("VirtualChannelStream.EndRead");
				StreamAsyncResult result = (StreamAsyncResult)asyncResult;
				AsyncReadCaller caller = (AsyncReadCaller)result.caller;
				return caller.EndInvoke(result.invokeResult);
			}

			public SafeFileHandle streamhandle;

			public override IAsyncResult BeginWrite(byte[] buffer, int offset, int count, AsyncCallback callback, object state)
			{
				if (streamhandle.IsClosed)
				{
					throw new ObjectDisposedException("For test");
				}
				if (WriteException != null)
				{
					var ex = WriteException;
					WriteException = null;
					throw ex;
				}

				Debug.WriteLine("VirtualChannelStream.BeginWrite n=" + count);
				StreamAsyncResult result = new StreamAsyncResult();
				var caller = new AsyncWriteCaller(Write);
				result.caller = caller;
				result.invokeResult = caller.BeginInvoke(buffer, offset, count, callback, state);
				return result;
			}

			public override void EndWrite(IAsyncResult asyncResult)
			{
				Debug.WriteLine("VirtualChannelStream.EndWrite");
				StreamAsyncResult result = (StreamAsyncResult)asyncResult;
				AsyncWriteCaller caller = (AsyncWriteCaller)result.caller;
				caller.EndInvoke(result.invokeResult);
			}

			public override int Read(byte[] buffer, int offset, int count)
			{
				Debug.WriteLine("VirtualChannelStream.Read");
				int length;
				if (readDataStream != null)
				{
					length = readDataStream.Read(buffer, 8, count - 8) + 8;
					if (readDataStream.Position < readDataStream.Length)
					{
						Array.Copy(BitConverter.GetBytes((int)WtsApi.ChannelFlags.Middle), 0, buffer, 4, 4);
					}
					else
					{
						Array.Copy(BitConverter.GetBytes((int)WtsApi.ChannelFlags.Last), 0, buffer, 4, 4);
						readDataStream = null;
					}
				}
				else
				{
					string line = null;
					length = 0;
					while (line == null && !isClosed)
					{
						if (ReadException != null)
						{
							var ex = ReadException;
							ReadException = null;
							throw ex;
						}

						lock (outputLines)
						{
							if (outputLines.Count > 0)
							{
								line = outputLines.Dequeue();
							}
						}

						if (line == null)
						{
							Thread.Sleep(50);
						}
					}

					if (!isClosed)
					{
						byte[] readData = Convert.FromBase64String(line);

						if (IsRawOutput)
						{
							length = readData.Length;
							Array.Copy(readData, 0, buffer, 0, readData.Length);
						}
						else
						{
							if (readData.Length + 8 <= count)
							{
								length = readData.Length + 8;
								Array.Copy(BitConverter.GetBytes((int)WtsApi.ChannelFlags.Only), 0, buffer, 4, 4);
								Array.Copy(readData, 0, buffer, 8, readData.Length);
							}
							else
							{
								readDataStream = new MemoryStream(readData);
								length = readDataStream.Read(buffer, 8, count - 8) + 8;
								Array.Copy(BitConverter.GetBytes((int)WtsApi.ChannelFlags.First), 0, buffer, 4, 4);
							}
						}
					}
				}
				Debug.WriteLine("VirtualChannelStream.Read returns n=" + length);
				return length;
			}

			Stream readDataStream;

			public override void Write(byte[] buffer, int offset, int count)
			{
				Debug.WriteLine("VirtualChannelStream.Write n=" + count);

				clientProcessStandardInput.WriteLine(Convert.ToBase64String(buffer, offset, count));
			}

			public override bool CanRead
			{
				get { return true; }
			}

			public override bool CanSeek
			{
				get { return false; }
			}

			public override bool CanWrite
			{
				get { return true; }
			}

			public override void Flush()
			{
			}

			public override long Length
			{
				get { throw new NotImplementedException(); }
			}

			public override long Position
			{
				get
				{
					throw new NotImplementedException();
				}
				set
				{
					throw new NotImplementedException();
				}
			}

			public override long Seek(long offset, SeekOrigin origin)
			{
				throw new NotImplementedException();
			}

			public override void SetLength(long value)
			{
				throw new NotImplementedException();
			}
		}

		public bool VirtualChannelClose(IntPtr channelHandle)
		{
			return true;
		}

		public bool RegisterSessionNotification(IntPtr hWnd)
		{
			SessionNotificationHandle = hWnd;
			SessionNotificationHandleDispatcher = Dispatcher.CurrentDispatcher;
			return true;
		}

		public bool UnregisterSessionNotification(IntPtr hWnd)
		{
			SessionNotificationHandle = IntPtr.Zero;
			SessionNotificationHandleDispatcher = null;
			return true;
		}

		public IntPtr SessionNotificationHandle
		{
			get;
			private set;
		}

		public Dispatcher SessionNotificationHandleDispatcher
		{
			get;
			private set;
		}
	}
}
