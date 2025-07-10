using System.Collections;
using System.Collections.Generic;
using CargoWise.Common;
using Enterprise.DocumentVisualizer.Core;

namespace Enterprise.DocumentVisualizer.FlexCelIntegration
{
	sealed class FlexCelDrawingsCollection : IReadOnlyList<IDrawing>
	{
		public FlexCelDrawingsCollection(FlexCelWorksheet worksheet)
		{
			this.worksheet = Argument.NotNull(worksheet, "worksheet");
		}

		readonly FlexCelWorksheet worksheet;
		Dictionary<int, IDrawing> drawings;

		public IEnumerator<IDrawing> GetEnumerator()
		{
			for (int i = 1; i <= worksheet.ImageCount; i++)
			{
				yield return GetAt(i);
			}
		}

		IEnumerator IEnumerable.GetEnumerator()
		{
			IEnumerable<IDrawing> enumerable = this;
			return enumerable.GetEnumerator();
		}

		public IDrawing this[int index]
		{
			get { return GetAt(index + 1); }
		}

		// uses 1 based index
		IDrawing GetAt(int drawingNumber)
		{
			IDrawing result = null;

			if (drawings == null || !drawings.ContainsKey(drawingNumber))
			{
				result = CreateDrawing(drawingNumber);

				if (drawings == null)
				{
					drawings = new Dictionary<int, IDrawing>();
				}

				drawings[drawingNumber] = result;
			}
			else
			{
				result = drawings[drawingNumber];
			}

			return result;
		}

		IDrawing CreateDrawing(int drawingNumber)
		{
			var isOutOfBounds = drawingNumber <= 0 || drawingNumber > worksheet.ImageCount;

			if (isOutOfBounds)
			{
				return new Drawing();
			}

			return new FlexCelDrawing(worksheet, drawingNumber);
		}

		public int Count
		{
			get { return worksheet.ImageCount; }
		}
	}
}