using System;
using System.Collections.Generic;
using System.Linq;

namespace Enterprise.DocumentVisualizer.GUI
{
	delegate bool DrawText(string[] words, IReadOnlyDictionary<string, float> widths, float spaceBetweenWords);

	sealed class JustifiedTextDrawHelper
	{
		public JustifiedTextDrawHelper(Func<string, float> measureText, DrawText drawText)
		{
			this.measureText = measureText;
			this.drawText = drawText;
		}

		readonly Func<string, float> measureText;
		readonly DrawText drawText;

		public void Draw(string text, float availableWidth, float spaceWidth)
		{
			const float precission = 0.01f;

			var paragraphs = text.Split(new[] { "\r\n", "\n" }, StringSplitOptions.None);

			foreach (var paragraph in paragraphs)
			{
				var wordsInParagraph = paragraph.Split(' ').Where(x => !string.IsNullOrWhiteSpace(x));
				var words = new Queue<string>(wordsInParagraph.Any() ? wordsInParagraph : new string[] { "" });

				var widths = new Dictionary<string, float>();

				var accumulator = new List<string>();
				var currentLineWidth = 0f;

				while (words.Any())
				{
					var word = words.Peek();
					var wordWidth = 0f;

					if (!widths.TryGetValue(word, out wordWidth))
					{
						wordWidth = measureText(word);
						widths[word] = wordWidth;
					}

					var numberOfWords = accumulator.Count;
					var numberOfSpaces = numberOfWords > 1
						? numberOfWords - 1
						: 0;

					var canFit = availableWidth - (currentLineWidth + wordWidth + numberOfSpaces * spaceWidth) > precission;
					bool isEndOfText = false;

					if (canFit)
					{
						currentLineWidth += wordWidth;
						accumulator.Add(word);
						words.Dequeue();

						if (words.Any())
						{
							continue;
						}
						else
						{
							isEndOfText = true;
						}
					}

					var spaceBetweenWords = numberOfSpaces == 0 ? 0 : (availableWidth - currentLineWidth) / numberOfSpaces;

					if (isEndOfText)
					{
						if (!drawText(accumulator.ToArray(), widths, spaceWidth))
						{
							return;
						}
					}
					else
					{
						if (!drawText(accumulator.ToArray(), widths, spaceBetweenWords))
						{
							return;
						}
					}

					accumulator.Clear();
					currentLineWidth = 0f;
				}
			}
		}
	}
}