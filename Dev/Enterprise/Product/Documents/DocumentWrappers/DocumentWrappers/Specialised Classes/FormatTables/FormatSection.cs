using System;
using System.Collections.Generic;

namespace Enterprise.DocumentWrappers.FormatTables
{
	internal sealed class FormatSection : IDisposable
	{
		public FormatSection(int windowHeight, IEnumerable<IFormatSectionComponent> tables)
		{
			if (tables == null)
			{
				throw new ArgumentNullException(nameof(tables));
			}

			this.windowHeight = windowHeight;
			this.tables = tables;
		}

		public int WindowHeight
		{
			[System.Diagnostics.DebuggerStepThrough]
			get { return windowHeight; }
		}

		public void Reset()
		{
			if (disposed)
			{
				throw new ObjectDisposedException("FormatSection");
			}

			ResetCore();
		}

		public List<string> ReadPage()
		{
			if (disposed)
			{
				throw new ObjectDisposedException("FormatSection");
			}

			if (currentLine == null && !isEndOfSection)
			{
				MoveToFirstLine();
			}

			if (isEndOfSection)
			{
				return null;
			}
			else
			{
				bool needsHeader = currentTable.Current.IncludeHeadingInBody;
				bool needsSpacer = false;
				List<string> result = new List<string>(windowHeight);

				while (!isEndOfSection && result.Count < windowHeight)
				{
					if (needsHeader)
					{
						if (header == null)
						{
							header = new List<string>(currentTable.Current.Heading);
						}

						int required = header.Count + (needsSpacer ? 1 : 0);

						if ((result.Count + required) < windowHeight)
						{
							if (needsSpacer)
							{
								result.Add(string.Empty);
								needsSpacer = false;
							}

							result.AddRange(header);
							needsHeader = false;
						}
						else
						{
							break;
						}
					}
					else if (needsSpacer)
					{
						if ((result.Count + 1) < windowHeight)
						{
							result.Add(string.Empty);
							needsSpacer = false;
						}
						else
						{
							break;
						}
					}

					result.Add(currentLine.Current);

					if (MoveToNextLine())
					{
						needsHeader = currentTable.Current.IncludeHeadingInBody;
						needsSpacer = true;
					}
				}

				return result.Count == 0 ? null : result;
			}
		}

		public List<string> ReadFollowOn()
		{
			if (disposed)
			{
				throw new ObjectDisposedException("FormatSection");
			}

			if (currentLine == null && !isEndOfSection)
			{
				MoveToFirstLine();
			}

			if (isEndOfSection)
			{
				return null;
			}
			else
			{
				bool needsHeader = true;
				List<string> result = new List<string>();

				while (!isEndOfSection)
				{
					if (needsHeader)
					{
						if (header == null)
						{
							header = new List<string>(currentTable.Current.Heading);
						}

						if (result.Count > 0)
						{
							result.Add(string.Empty);
						}

						result.AddRange(header);
						needsHeader = false;
					}

					result.Add(currentLine.Current);

					if (MoveToNextLine())
					{
						needsHeader = true;
					}
				}

				return result.Count == 0 ? null : result;
			}
		}

		#region IDisposable Members

		public void Dispose()
		{
			ResetCore();
			disposed = true;
		}

		#endregion

		#region Implementation

		void MoveToFirstLine()
		{
			if (currentTable != null)
			{
				currentTable.Dispose();
			}

			currentTable = tables.GetEnumerator();
			header = null;

			do
			{
				if (currentTable.MoveNext())
				{
					if (currentLine != null)
					{
						currentLine.Dispose();
					}

					currentLine = currentTable.Current.Body.GetEnumerator();

					if (currentLine.MoveNext())
					{
						isEndOfSection = false;
						break;
					}
				}
				else
				{
					isEndOfSection = true;
					break;
				}
			}
			while (true);
		}

		/// <summary>
		/// Moves to the next line, switching to the next table if necessisary.
		/// </summary>
		/// <returns>true if moved to the next table, false otherwise.</returns>
		bool MoveToNextLine()
		{
			if (currentLine == null)
			{
				throw new InvalidOperationException("Not started yet!");
			}

			if (isEndOfSection)
			{
				return false;
			}
			else
			{
				bool isNewTable = false;

				while (!currentLine.MoveNext())
				{
					currentLine.Dispose();

					if (currentTable.MoveNext())
					{
						currentLine = currentTable.Current.Body.GetEnumerator();
						header = null;
						isNewTable = true;
					}
					else
					{
						currentTable.Dispose();
						currentTable = null;
						currentLine = null;
						isEndOfSection = true;
						return false;
					}
				}

				return isNewTable;
			}
		}

		void ResetCore()
		{
			if (currentLine != null)
			{
				currentLine.Dispose();
				currentLine = null;
			}

			if (currentTable != null)
			{
				currentTable.Dispose();
				currentTable = null;
			}

			header = null;
			isEndOfSection = false;
		}

		[System.Diagnostics.DebuggerBrowsable(System.Diagnostics.DebuggerBrowsableState.Never)]
		bool disposed;
		[System.Diagnostics.DebuggerBrowsable(System.Diagnostics.DebuggerBrowsableState.Never)]
		bool isEndOfSection;
		[System.Diagnostics.DebuggerBrowsable(System.Diagnostics.DebuggerBrowsableState.Never)]
		List<string> header;
		[System.Diagnostics.DebuggerBrowsable(System.Diagnostics.DebuggerBrowsableState.Never)]
		IEnumerator<string> currentLine;
		[System.Diagnostics.DebuggerBrowsable(System.Diagnostics.DebuggerBrowsableState.Never)]
		IEnumerator<IFormatSectionComponent> currentTable;
		[System.Diagnostics.DebuggerBrowsable(System.Diagnostics.DebuggerBrowsableState.RootHidden)]
		readonly IEnumerable<IFormatSectionComponent> tables;
		[System.Diagnostics.DebuggerBrowsable(System.Diagnostics.DebuggerBrowsableState.Never)]
		readonly int windowHeight;

		#endregion
	}
}
