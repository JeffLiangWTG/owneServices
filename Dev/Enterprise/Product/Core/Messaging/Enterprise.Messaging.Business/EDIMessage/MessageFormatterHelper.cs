using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using CargoWise.IO;

namespace Enterprise.Messaging.Business
{
	// TODO: We should refactor this class to use SubStreamableStream as stream processing can then be performed more effeciently without copying, however to do this
	// EDIMessage must first be refactored to properly dispose of the streams it creates.
	public static class MessageFormatterHelper
	{
		public static Stream AddHeader(this Stream stream, string header)
		{
			stream.Position = 0;
			var formattedStream = new VirtualMemoryStream();
			var reader = new StreamReader(stream);
			var writer = new StreamWriter(formattedStream);

			writer.Write(header);
			writer.AddStream(reader);

			writer.Flush();
			stream.Dispose();

			formattedStream.Position = 0;
			return formattedStream;
		}

		public static void AddFooter(this Stream stream, string footer)
		{
			stream.Position = stream.Length;
			var writer = new StreamWriter(stream);
			writer.Write(footer);
			writer.Flush();
			stream.Position = 0;
		}

		public static Stream ReplaceTextBlock(this Stream stream, string startBlockString, string endBlockString, string replaceString, int bufferSize = LargeMessageHelper.BufferSize)
		{
			var formattedStream = new VirtualMemoryStream();
			stream.Position = 0;
			var reader = new StreamReader(stream);
			var writer = new StreamWriter(formattedStream);

			char[] buffer1 = new char[bufferSize];
			char[] buffer2 = new char[bufferSize];

			int read = reader.Read(buffer1, 0, buffer1.Length);
			if (read != buffer1.Length)
			{
				Array.Resize<char>(ref buffer1, read);
			}

			bool isSkipping = false;
			bool isReplaced = false;

			while (true)
			{
				read = reader.Read(buffer2, 0, buffer2.Length);
				int buffer2Length = buffer2.Length;
				if (read != buffer2.Length)
				{
					Array.Resize<char>(ref buffer2, read);
				}

				if (!isReplaced)
				{
					string sumString = new string(buffer1) + new string(buffer2);

					if (isSkipping)
					{
						int endSkipIndex = sumString.IndexOf(endBlockString, StringComparison.InvariantCulture);
						if (endSkipIndex != -1)
						{
							writer.Write(replaceString);
							buffer1 = sumString.Substring(endSkipIndex).ToCharArray();
							isSkipping = false;
							isReplaced = true;
							continue;
						}
					}

					int startSkipIndex = sumString.IndexOf(startBlockString, StringComparison.InvariantCulture);
					if (startSkipIndex != -1)
					{
						isSkipping = true;
						writer.Write(sumString.ToCharArray(), 0, startSkipIndex + startBlockString.Length);
						buffer1 = sumString.Substring(startSkipIndex + startBlockString.Length).ToCharArray();
						continue;
					}
				}

				writer.Write(buffer1);
				buffer1 = buffer2;

				if (read == 0 || read != buffer2Length)
				{
					break;
				}
			}

			writer.Write(buffer2);
			writer.Flush();
			stream.Dispose();

			formattedStream.Position = 0;
			return formattedStream;
		}

		/// <summary>
		/// Doesn't work in the same way as String.Replace for some cases on the edge
		/// </summary>
		/// <param name="stream"></param>
		/// <param name="replacePairs"></param>
		/// <param name="bufferSize"></param>
		/// <returns></returns>
		public static Stream ReplaceStrings(this Stream stream, KeyValuePair<string, string>[] replacePairs, int bufferSize = LargeMessageHelper.BufferSize)
		{
			int maxLength = replacePairs.Max(n => n.Key.Length);

			var formattedStream = new VirtualMemoryStream();
			stream.Position = 0;
			var reader = new StreamReader(stream);
			var writer = new StreamWriter(formattedStream);

			char[] buffer = new char[bufferSize];
			int skipFirstCharCount = 0;

			while (true)
			{
				int read = reader.Read(buffer, skipFirstCharCount, buffer.Length - skipFirstCharCount);
				if (read == buffer.Length - skipFirstCharCount)
				{
					string longString = new string(buffer);
					string shortString = new string(buffer, 0, buffer.Length - maxLength);

					string formattedLongString = longString.ReplaceStringList(replacePairs);
					string formattedShortString = shortString.ReplaceStringList(replacePairs);

					int cutPosition = formattedShortString.Length;

					for (int i = 0; i < formattedShortString.Length; i++)
					{
						if (formattedLongString[i] != formattedShortString[i])
						{
							cutPosition = i;
							break;
						}
					}

					char[] formattedLongBytes = formattedLongString.ToCharArray();
					writer.Write(formattedLongBytes, 0, cutPosition);
					skipFirstCharCount = formattedLongBytes.Length - cutPosition;
					Array.Copy(formattedLongBytes, cutPosition, buffer, 0, skipFirstCharCount);
				}
				else
				{
					Array.Resize<char>(ref buffer, read + skipFirstCharCount);
					writer.Write((new string(buffer).ReplaceStringList(replacePairs)));
					break;
				}
			}

			writer.Flush();
			stream.Dispose();

			formattedStream.Position = 0;
			return formattedStream;
		}

		public static string ReplaceStringList(this string text, KeyValuePair<string, string>[] replacePairs)
		{
			var textBuilder = new StringBuilder(text);

			for (int i = 0; i < replacePairs.Length; i++)
			{
				textBuilder.Replace(replacePairs[i].Key, replacePairs[i].Value);
			}

			return textBuilder.ToString();
		}

		public static Stream ReplaceChars(this Stream stream, KeyValuePair<char, string>[] replacePairs)
		{
			var formattedStream = new VirtualMemoryStream();

			stream.Position = 0;
			var reader = new StreamReader(stream);
			var writer = new StreamWriter(formattedStream);

			char[] buffer = new char[LargeMessageHelper.BufferSize];

			while (true)
			{
				int read = reader.Read(buffer, 0, buffer.Length);
				int bufferLength = buffer.Length;
				if (read != buffer.Length)
				{
					Array.Resize<char>(ref buffer, read);
				}

				ReplaceCharsInBufferAndAddToStream(buffer, writer, replacePairs);

				if (read != bufferLength)
				{
					break;
				}
			}

			writer.Flush();
			stream.Dispose();

			formattedStream.Position = 0;
			return formattedStream;
		}

		static void ReplaceCharsInBufferAndAddToStream(char[] buffer, StreamWriter writer, KeyValuePair<char, string>[] replacePairs)
		{
			for (int i = 0; i < buffer.Length; i++)
			{
				bool isProcessed = false;

				for (int j = 0; j < replacePairs.Length; j++)
				{
					if (buffer[i] == replacePairs[j].Key)
					{
						writer.Write(replacePairs[j].Value);
						isProcessed = true;
					}
				}

				if (!isProcessed)
				{
					writer.Write(buffer[i]);
				}
			}
		}

		public static Stream AddStringAfterEachNumberOfChars(this Stream stream, int charsNumber, string stringToAdd)
		{
			var formattedStream = new VirtualMemoryStream();

			stream.Position = 0;
			var reader = new StreamReader(stream);
			var writer = new StreamWriter(formattedStream);

			char[] buffer = new char[charsNumber];

			while (true)
			{
				int read = reader.Read(buffer, 0, buffer.Length);
				int bufferLength = buffer.Length;
				if (read != buffer.Length)
				{
					Array.Resize<char>(ref buffer, read);
				}

				writer.Write(buffer);
				if (read == bufferLength)
				{
					writer.Write(stringToAdd);
				}
				else
				{
					break;
				}
			}

			writer.Flush();
			stream.Dispose();

			formattedStream.Position = 0;
			return formattedStream;
		}
	}
}
