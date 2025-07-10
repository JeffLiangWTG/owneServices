using System.Linq;

namespace Enterprise.Customs.FR.Business.CusTempStorage
{
	public class LADTCusTempStorageDecValidation : CusTempStorageDecValidation
	{
		public LADTCusTempStorageDecValidation(LADTCusTempStorageDec parent) : base(parent)
		{
		}

		protected new LADTCusTempStorageDec Parent => (LADTCusTempStorageDec)base.Parent;

		public override void ValidateAll()
		{
			base.ValidateAll();
			CheckMandatoryLine();
		}

		void CheckMandatoryLine()
		{
			if (Parent.CusTempStorageLines.Any())
			{
				Parent.RemoveRowError(AtLeastOneLineIsMandatory);
			}
			else
			{
				Parent.AddRowError(AtLeastOneLineIsMandatory);
			}
		}

		static string AtLeastOneLineIsMandatory => Res.GetString("82C3E1E8-9BDA-4CA0-99BD-0DD3224E50EC", "You need to supply at least one line.");
	}
}
