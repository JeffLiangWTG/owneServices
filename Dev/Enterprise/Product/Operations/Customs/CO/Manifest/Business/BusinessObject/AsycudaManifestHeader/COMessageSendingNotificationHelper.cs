using System.Linq;
using CargoWise.Types;
using Enterprise.Customs.ASYCUDA.Business;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Core;

namespace Enterprise.Customs.CO.Manifest.Business
{
	internal class COMessageSendingNotificationHelper : MessageSendingNotificationHelper
	{
		public COMessageSendingNotificationHelper(AsycudaManifestHeader header) : base(header)
		{
		}

		static MultilingualString NoBillAdded => ResString.GetMultilingualString("AFD03590-A0B8-4E60-B304-09F18FC7710F",
			"No Bills have been added. Please add their information so that the Manifest can be sent.");

		static MultilingualString NoPackAdded => ResString.GetMultilingualString("99F26E73-1871-44BF-B9CA-0958E73848CA",
			"No Packs have been added. Please add their information so that the Manifest can be sent.");

		static MultilingualString NoCompanyTaxIDFilled => ResString.GetMultilingualString("4E352A90-5C33-415F-A599-F8CFA5A97F2B",
			"You have not entered a Company Tax ID. You can enter it from Maintain -> User Admin -> Companies. Picking the company, NIT Reg No field.");

		protected override ZString GetExtraMessageSendingNotificationCore()
		{
			if (header.Bills.Count == 0)
			{
				return (ZString)NoBillAdded;
			}
			else if (header.Bills.Cast<AsycudaBill>().Sum(x => x.Packs.Count) == 0)
			{
				return (ZString)NoPackAdded;
			}
			else if (GlbCompany.CurrentCompany.GC_BusinessRegNo.IsEmpty)
			{
				return (ZString)NoCompanyTaxIDFilled;
			}
			else
			{
				return base.GetExtraMessageSendingNotificationCore();
			}
		}
	}
}
