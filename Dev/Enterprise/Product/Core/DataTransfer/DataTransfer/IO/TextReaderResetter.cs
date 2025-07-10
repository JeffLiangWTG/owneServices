using System;
using System.IO;
using CargoWise.Common;
using CargoWise.Common.Testing;

namespace Enterprise.DataTransfer.IO
{
	public abstract class TextReaderResetter : IDisposable
	{
		protected TextReaderResetter(TextReader reader)
		{
			fReader = InitialiseReader(reader);
			DisposableLeakListener.Instance.RegisterDisposable(this);
		}

		public static TextReaderResetter New(TextReader reader)
		{
			TextReaderResetter result;
			if (reader is StringReader)
			{
				result = new StringReaderReseter(reader as StringReader);
			}
			else if (reader is StreamReader)
			{
				result = new StreamReaderReseter(reader as StreamReader);
			}
			else
			{
				throw new ArgumentException("Unsupported TextReader type " + reader.GetType().FullName);
			}
			return result;
		}

		public TextReader Reader
		{
			get
			{
				if (!WasReset)
				{
					ReaderWasAccessedBeforeReset = true;
				}
				return fReader;
			}
		}
		public TextReader fReader;

		public TextReader Reset()
		{
			WasReset = true;
			return ResetCore();
		}

		protected abstract TextReader ResetCore();
		protected abstract TextReader InitialiseReader(TextReader reader);

		#region IDisposable Members

		void IDisposable.Dispose()
		{
			DisposableLeakListener.Instance.UnRegisterDisposable(this);
			if (!WasReset)
			{
				ErrorReporter.ReportOnce("TextReaderResetter.ResetNotCalled", "Reset() must be called before disposing TextReaderResetter");
			}
			if (!ReaderWasAccessedBeforeReset)
			{
				ErrorReporter.ReportOnce("TextReaderResetter.ReaderNotAccessedBeforeReset", "The reader returned from the Reader property must be accessed before Reset() is called, because, in the case of StringReader, it may be a different Reader than what was passed into the constructor");
			}
		}

		#endregion

		#region StreamReaderReseter

		class StreamReaderReseter : TextReaderResetter
		{
			public StreamReaderReseter(StreamReader reader)
				: base(reader)
			{
			}

			protected override TextReader InitialiseReader(TextReader textReader)
			{
				StreamReader reader = (StreamReader)textReader;
				if (reader.BaseStream.Position != 0)
				{
					throw new InvalidOperationException("Not currently supported if the stream's position is not zero");
				}
				return reader;
			}

			protected override TextReader ResetCore()
			{
				Reader.BaseStream.Position = 0;
				Reader.DiscardBufferedData();
				return Reader;
			}

			protected new StreamReader Reader
			{
				get { return (StreamReader)base.Reader; }
			}
		}

		#endregion

		#region StringReaderReseter

		class StringReaderReseter : TextReaderResetter
		{
			public StringReaderReseter(StringReader reader)
				: base(reader)
			{
			}

			protected override TextReader InitialiseReader(TextReader reader)
			{
				Content = reader.ReadToEnd();
				return new StringReader(Content);
			}

			protected override TextReader ResetCore()
			{
				return new StringReader(Content);
			}

			string Content;
		}

		#endregion

		bool WasReset;
		bool ReaderWasAccessedBeforeReset;
	}
}
