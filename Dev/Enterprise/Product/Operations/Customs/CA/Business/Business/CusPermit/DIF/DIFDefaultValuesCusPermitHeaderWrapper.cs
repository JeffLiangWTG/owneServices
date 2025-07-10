using CargoWise.Common;
using CargoWise.Types;
using Enterprise.MasterFiles.Business.DIS;

namespace Enterprise.Customs.CA.Business
{
	public class DIFDefaultValuesCusPermitHeaderWrapper : ICADIFDefaultValues
	{
		public DIFDefaultValuesCusPermitHeaderWrapper(CusPermitHeader cusPermitHeader)
		{
			this.cusPermitHeader = Argument.NotNull(cusPermitHeader, nameof(cusPermitHeader));
		}
		readonly CusPermitHeader cusPermitHeader;

		ZString ICADIFDefaultValues.DocumentNumber => cusPermitHeader.CPH_Number;

		ZDate ICADIFDefaultValues.EffectiveDate => cusPermitHeader.CPH_StartDate;

		ZDate ICADIFDefaultValues.ExpiryDate => cusPermitHeader.CPH_EndDate;
	}
}
