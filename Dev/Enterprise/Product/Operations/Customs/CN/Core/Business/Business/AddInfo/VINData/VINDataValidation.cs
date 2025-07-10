using System.Linq;
using Enterprise.Customs.Business.MultiLineAddInfos;

namespace Enterprise.Customs.CN.Business
{
	public class VINDataValidation : CusAddInfoValidation
	{
		public VINDataValidation(VINData parent) : base(parent)
		{
		}

		internal IValidationModeProvider ValidationModeProvider => Parent.Parent;

		protected new VINData Parent => (VINData)base.Parent;

		public override void ValidateAll()
		{
			base.ValidateAll();
			CheckParentSupportsVIN();
		}

		protected override void CheckB7_AddInfoDataIsWesternEuropean()
		{
		}

		static string NoVINDataRequired => Res.GetString("E4639E2F-73A6-496E-BB3B-8F857DF2B445", "No VIN data is required if there is no CIQ Product Qualification with Type 408/409/603.");

		void CheckParentSupportsVIN()
		{
			if (Parent.Parent != null)
			{
				Parent.RemoveRowMessageError(NoVINDataRequired);
				var pqs = Parent.Parent.CIQProductQualifications.Cast<CIQProductQualification>();
				if (pqs.All(pq => !pq.SupportsVIN))
				{
					Parent.AddRowNotification(NoVINDataRequired, ValidationModeProvider);
				}
			}
		}
	}
}
