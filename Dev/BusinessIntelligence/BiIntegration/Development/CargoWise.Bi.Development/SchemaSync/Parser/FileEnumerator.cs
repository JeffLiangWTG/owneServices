using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;

namespace CargoWise.Bi.Development.SchemaSync.Parser;

public class FileEnumerator : IEnumerable<TextReader>, IEnumerator<TextReader>
{
	readonly string[] fileNames;
	int index;

	TextReader currentStream;

	public FileEnumerator(string directory, string searchPattern)
	{
		fileNames = Directory.GetFiles(directory, searchPattern, SearchOption.AllDirectories);
		index = -1;
	}

	public TextReader Current => currentStream;

	object IEnumerator.Current => Current;

	public void Dispose() { }

	public IEnumerator<TextReader> GetEnumerator()
	{
		return this;
	}

	public bool MoveNext()
	{
		if (currentStream != null)
		{
			currentStream.Dispose();
			currentStream = null;
		}

		if (++index >= fileNames.Length)
		{
			return false;
		}

		currentStream = new StreamReader(fileNames[index]);
		return true;
	}

	public void Reset()
	{
		throw new NotSupportedException("FileEnumerator does not support resetting, this will result in reading all files again.");
	}

	IEnumerator IEnumerable.GetEnumerator()
	{
		return GetEnumerator();
	}
}
