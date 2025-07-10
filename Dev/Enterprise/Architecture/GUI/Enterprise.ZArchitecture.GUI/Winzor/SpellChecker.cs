using System.Collections;
using System.Linq;
using Enterprise.ZArchitecture.GUI.JSInterop;
using Microsoft.JSInterop;

namespace Enterprise.ZArchitecture.GUI.Tools
{
	public partial class SpellChecker
	{
		readonly DotNetObjectReference<SpellChecker> dotNetObjectReference;

		internal ISpellCheckerJSInterop Interop => textbox.GetJSInterop<ISpellCheckerJSInterop>();

		[JSInvokable]
		public IEnumerable CheckTextSpelling(string text)
		{
			var spellingErrors = spellChecker.CheckSpelling(text);
			var squiggleRanges = spellingErrors?.Select(error => new SquiggleRange { Start = error.WordIndex, End = error.WordIndex + error.Word.Length });
			return squiggleRanges;
		}

		public record SquiggleRange
		{
			public int Start { get; set; }
			public int End { get; set; }
		}
	}
}
