using System.Collections.Generic;
using CargoWise.Common;
using CargoWise.Types;
using Enterprise.Customs.Business;

namespace Enterprise.Customs.EU.NCTS.Business
{
	sealed class CustomsOfficesNumberGenerator : ISequenceNumberHeader
	{
		public CustomsOfficesNumberGenerator(NctsHeader header)
		{
			this.header = Argument.NotNull(header, nameof(header));
			generator = new ShortSequenceNumberGenerator(this, x => ((NctsEuOfficeCode)x).CY_Code == currentCY_Code);
		}

		public CustomsOfficesNumberGenerator(NctsCommonMovementHeader movementHeader)
		{
			this.movementHeader = Argument.NotNull(movementHeader, nameof(movementHeader));
			generator = new ShortSequenceNumberGenerator(this, x => ((NctsEuOfficeCode)x).CY_Code == currentCY_Code);
		}

		readonly NctsHeader header;
		readonly NctsCommonMovementHeader movementHeader;
		readonly ShortSequenceNumberGenerator generator;

		readonly object currentLock = new object();
		ZString currentCY_Code;

		IEnumerable<ISequenceNumberLine> ISequenceNumberHeader.Lines
		{
			get
			{
				if (movementHeader != null)
				{
					return movementHeader.CustomsOffices;
				}
				else
				{
					return header.CustomsOffices;
				}
			}
		}

		public void RecalculateWhenAdded(IShortSequenceNumberLine line, ZString subType)
		{
			lock (currentLock)
			{
				currentCY_Code = subType;
				generator.RecalculateWhenAdded(line);
			}
		}

		public void RecalculateWhenRenumbered(IShortSequenceNumberLine line, ZShort oldValue, ZString subType)
		{
			lock (currentLock)
			{
				currentCY_Code = subType;
				generator.RecalculateWhenRenumbered(line, oldValue);
			}
		}

		public void RecalculateWhenAboutToBeDetachedOrDeleted(IShortSequenceNumberLine line, ZString subType)
		{
			lock (currentLock)
			{
				currentCY_Code = subType;
				generator.RecalculateWhenAboutToBeDetachedOrDeleted(line);
			}
		}
	}
}
