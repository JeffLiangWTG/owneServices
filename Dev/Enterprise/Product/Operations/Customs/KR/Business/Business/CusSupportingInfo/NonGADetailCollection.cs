using System.Linq;
using CargoWise.EntityFramework;
using Enterprise.Customs.Business;

namespace Enterprise.Customs.KR.Business
{
	public class NonGADetailCollection : CusSupportingInfoCollection<NonGADetail>
	{
		public NonGADetailCollection(ILineOrProduct parent)
			: base((BusinessObject)parent, CusSupportingInfoTypeList.Codes.NonGADetail)
		{
		}

		public void AddNewIfRequired(NonGADetail nonGADetail)
		{
			if (!this.Cast<NonGADetail>().Any(x => x.HasSameKey(nonGADetail)))
			{
				var @new = AddNew();
				@new.CSI_Procedure = nonGADetail.CSI_Procedure;
				@new.CSI_Code = nonGADetail.CSI_Code;
				@new.CSI_Status = nonGADetail.CSI_Status;
				@new.CSI_Description = nonGADetail.CSI_Description;
			}
		}
	}
}
