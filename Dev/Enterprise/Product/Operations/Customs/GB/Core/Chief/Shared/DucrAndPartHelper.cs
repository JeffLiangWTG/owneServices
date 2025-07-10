using System.Text.RegularExpressions;
using CargoWise.Types;

namespace Enterprise.Customs.GB.Chief
{
	/// <summary>
	/// Gets the right DUCR and part suffix when given the crappy version returned by Chief.
	/// e.g.	9GB123456789000-B0001000/M		gives 9GB123456789000-B0001000
	///			9GB123456789000-B0001000/69		gives 9GB123456789000-B0001000/69
	///			9GB123456789000-B0001000/69M	gives 9GB123456789000-B0001000/69
	/// </summary>
	public class DucrAndPartHelper
	{
		public DucrAndPartHelper(ZString ducr, ZString ducrPart)
		{
			Ducr = ducr;
			this.part = ducrPart;
		}

		public ZString Ducr { get; private set; }

		public ZString DucrAndPartWithoutChecksum
		{
			get
			{
				if (PartWithoutCheckSum.IsEmpty)
				{
					return Ducr;
				}
				else
				{
					return string.Format("{0}/{1}", Ducr, PartWithoutCheckSum);
				}
			}
		}

		public ZString PartWithoutCheckSum
		{
			get
			{
				if (!part.IsEmpty)
				{
					// DECLN-UCR-PART is either up to three digits with an optional alpha checksum, or a lone alpha. If it's a lone alpha, ignore it.
					if (System.Text.RegularExpressions.Regex.IsMatch(part, "^[A-Z]$"))
					{
						// Ignore checksum
						return ZString.Empty;
					}
					else if (System.Text.RegularExpressions.Regex.IsMatch(part, "^[0-9]{1,3}$"))
					{
						// Digits only:
						return part;
					}
					else
					{
						Regex rego = new Regex("^(?<DIGITS>([0-9]{1,3}))[A-Z]$");
						MatchCollection matches = rego.Matches(part);
						if (matches != null & matches.Count > 0)
						{
							// Digits and alpha - must use only the digits
							return matches[0].Groups[1].Value;
						}
					}
				}
				return ZString.Empty;
			}
		}

		readonly ZString part;
	}
}
