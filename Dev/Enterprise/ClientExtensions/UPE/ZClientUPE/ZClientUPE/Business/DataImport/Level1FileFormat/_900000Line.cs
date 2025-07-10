

using System;
using System.Diagnostics.CodeAnalysis;
using System.Text.RegularExpressions;

namespace Enterprise.Client.UPE.Business.DataImport.Level1FileFormat
{
	[SuppressMessage("Microsoft.Design", "CA1052:StaticHolderTypesShouldNotHaveConstructors")]
	public class _900000Line : RecordLine
	{
		#region Constants

		[SuppressMessage("Microsoft.Design", "CA1053:StaticHolderTypesShouldNotHaveConstructors")]
		public new class Constants : RecordLine.Constants
		{
			public static class ARN
			{
				public const int Length = 69;
				public const int Position = 117;
			}
		}

		#endregion

		public _900000Line(string value)
			: base(value)
		{
		}

		public string ARNNumber
		{
			get
			{
				var arnString = Value.SubstringSafe(Constants.ARN.Position, Constants.ARN.Length).Replace(" ", string.Empty);
				if (ARNRex.IsMatch(arnString))
				{
					arnString = arnString.SubstringSafe(1, arnString.Length - 2);
					arnString = arnString.SubstringSafe(arnString.ToUpper().IndexOf("ARN:", StringComparison.OrdinalIgnoreCase) + 4);
				}
				else
				{
					arnString = string.Empty;
				}
				return arnString;
			}
		}

		public static readonly Regex ARNRex = new Regex(@"^(\s*#\s*)(ARN\s*:\s*){1}\w*(\s*#\s*)$", RegexOptions.Compiled | RegexOptions.IgnoreCase);
	}
}
