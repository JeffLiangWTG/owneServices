using CargoWise.Types;
using Enterprise.Customs.CA.Business;
using Enterprise.ZArchitecture.Environment;

namespace Enterprise.Customs.CA.Module
{
	public static class CreatePrecarmAdjustmentEntryHelper
	{
		public static JobDeclaration CreatePrecarmAdjustmentEntry(JobDeclaration sourceDeclaration)
		{
			JobDeclaration result = null;
			if (!sourceDeclaration.IsB3Lodged)
			{
				Globals.Message.ShowInformation(OnlyCreateFromB3Lodged, CreatePrecarmAdjustmentEntryCaption);
			}
			else
			{
				if (sourceDeclaration.B3AcceptedDate.IsValid && ZDateTime.Now.AddYears(-4) <= sourceDeclaration.B3AcceptedDate)
				{
					if (sourceDeclaration.HasPRECARMAdjustmentDeclaration())
					{
						Globals.Message.ShowInformation(PRECARMAdjustmentEntryAlreadyExistsMessage, CreatePrecarmAdjustmentEntryCaption);
					}
					else
					{
						result = sourceDeclaration.GetNewCopyToPRECARMAdjustmentDeclaration();
					}
				}
				else
				{
					Globals.Message.ShowInformation(OnlyCreateFromValidB3AcceptedDate, CreatePrecarmAdjustmentEntryCaption);
				}
			}
			return result;
		}

		internal static string CreatePrecarmAdjustmentEntryCaption
		{
			get { return ResString.GetMultilingualString("CD3B6D79-5D7C-43D6-9C4F-A635129D4CF8", "Create PRECARM adjustment entry"); }
		}

		static string OnlyCreateFromB3Lodged
		{
			get { return Res.GetString("F4D9A8AE-8952-4807-81CC-38552DC6DD4B", "You may only create PRECARM adjustment entry to B3 Lodged declarations."); }
		}

		static string OnlyCreateFromValidB3AcceptedDate
		{
			get { return Res.GetString("600F5EC3-CCEF-4BEC-A7A7-47DEE708BBE3", "You may not create PRECARM adjustment entry with B3 accept date older than 4 years."); }
		}

		static string PRECARMAdjustmentEntryAlreadyExistsMessage
		{
			get { return Res.GetString("DA6E3CB6-040B-48B9-BB8F-E70792700A85", "The PRECARM adjustment entry already exists."); }
		}
	}
}
