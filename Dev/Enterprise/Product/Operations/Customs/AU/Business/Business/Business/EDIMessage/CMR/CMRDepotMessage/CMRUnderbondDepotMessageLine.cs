using System;

using CargoWise.Types;

namespace Enterprise.Customs.AU.Declaration.Business
{
	public class CMRUnderbondDepotMessageLine : GenericCMRDepotMessageLine<CMRUBMREQRMessage>
	{
		public CMRUnderbondDepotMessageLine(CMRUBMREQRMessage message, ZInt lineNumber)
			: base(message)
		{
			if (lineNumber < 1 || lineNumber > message.Lines)
			{
				throw new ArgumentOutOfRangeException(nameof(lineNumber));
			}

			this.LineNumber = lineNumber;
		}

		protected override ZString GetContainerNumberCore()
		{
			return Message.GetContainerNumber(LineNumber);
		}

		protected override ZString GetGoodsDescriptionCore()
		{
			return Message.GetGoodsDescription(LineNumber);
		}

		protected override ZString GetHouseBillNumberCore()
		{
			return Message.GetHouseBillOfLading(LineNumber);
		}

		protected override ZString GetMarksAndNumbersCore()
		{
			return Message.GetMarksAndNumbers(LineNumber);
		}

		protected override ZInt GetNumberOfPackagesCore()
		{
			return Message.GetNumberOfPackages(LineNumber);
		}

		protected override ZString GetOceanBillNumberCore()
		{
			return Message.GetOceanBillOfLading(LineNumber);
		}

		protected override ZString GetPackageTypeCore()
		{
			return Message.GetPackageType(LineNumber);
		}

		protected override ZString GetContainerModeCore()
		{
			return Message.GetContainerMode(LineNumber);
		}

		protected override ZDecimal GetActualWeightCore()
		{
			return ZDecimal.Zero;
		}

		protected override ZString GetWeightUnitsCore()
		{
			return ZString.Empty;
		}

		protected override ZDecimal GetActualVolumeCore()
		{
			return ZDecimal.Zero;
		}

		protected override ZString GetVolumeUnitsCore()
		{
			return ZString.Empty;
		}

		protected readonly ZInt LineNumber;
	}
}
