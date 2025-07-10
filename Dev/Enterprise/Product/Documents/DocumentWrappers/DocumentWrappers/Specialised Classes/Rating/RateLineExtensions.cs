using CargoWise.Types;
using Enterprise.MasterFiles.Business;
using Enterprise.Rating.Business;

namespace Enterprise.DocumentWrappers
{
	internal static class RateLineExtensions
	{
		public static bool IsVisibleOnDocuments(this RateLine line)
		{
			if (line.ChargeCode.AC_ShowOnQuotation)
			{
				var result = !line.ChargeCode.AC_SuppressOnQuoteIfZero
					|| line.Calculator is CompanyTariffOrCostBasedCalculator
					|| line.HasValueForDocumentPrinting();

				return result;
			}

			return false;
		}

		/// <summary>
		/// If the line belongs to an origin entry with an origin port contained within the passed in location:
		/// then return it's name, otherwise return the name of the passed in location.
		/// </summary>
		internal static ZString ItemOrigin(this RateLine line, ILocation parentOrigin)
		{
			if (parentOrigin == null)
			{
				return ZString.Empty;
			}
			else if (line != null &&
				line.Parent != null &&
				line.Parent.IsOriginEntry() &&
				parentOrigin.CompletelyCovers(line.Parent.Origin()))
			{
				return line.Parent.Origin().Description;
			}
			else
			{
				return parentOrigin.Description;
			}
		}

		/// <summary>
		/// If the line belongs to a destination entry with a destination port contained within the passed in location:
		/// then return it's name, otherwise return the name of the passed in location.
		/// </summary>
		internal static ZString ItemDestination(this RateLine line, ILocation parentDestination)
		{
			if (parentDestination == null)
			{
				return ZString.Empty;
			}
			else if (line != null && line.Parent != null && line.Parent.IsDestinationEntry() && parentDestination.CompletelyCovers(line.Parent.Destination()))
			{
				return line.Parent.Destination().Description;
			}
			else
			{
				return parentDestination.Description;
			}
		}

		/// <summary>
		/// If the line belongs to an entry with a via port contained within the passed in location, then return it's name
		/// otherwise return the name of the passed in location.
		/// </summary>
		internal static ZString ItemVia(this RateLine line, ILocation parentVia)
		{
			if (parentVia == null)
			{
				return ZString.Empty;
			}
			else if (line != null && line.Parent != null && parentVia.CompletelyCovers(line.Parent.Via))
			{
				return line.Parent.Via.Description;
			}
			else
			{
				return parentVia.Description;
			}
		}
	}
}
