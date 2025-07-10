using System.Text.RegularExpressions;
using Enterprise.Customs.KR.Messaging;

namespace Enterprise.Customs.KR.Business
{
	public class LocalExportCusPersonValidation : CusPersonValidation
	{
		public LocalExportCusPersonValidation(CusPerson parent)
			: base(parent)
		{
		}

		protected override void CheckCPN_PER_Person()
		{
			base.CheckCPN_PER_Person();
			if (Parent.Person != null)
			{
				CheckBirthDate();
				CheckFullName();
			}
		}

		void CheckBirthDate()
		{
			if (LocalExportTransactionNatureCodeList.IsSea(Parent.ParentJobDeclaration.JE_MessageSubType) && Parent.PersonBirthDate.IsEmpty)
			{
				Parent.CPN_PER_PersonInfo.AddMessageError(Res.GetString("6585D88A-4560-4039-8910-0BB1665842F4", "You have not entered a birthday for this person."));
			}
		}

		void CheckFullName()
		{
			if (LocalExportTransactionNatureCodeList.IsSea(Parent.ParentJobDeclaration.JE_MessageSubType))
			{
				if (Parent.PersonFullName.IsEmpty)
				{
					Parent.CPN_PER_PersonInfo.AddMessageError(Res.GetString("8EFB24B4-95DA-4690-B6FE-B3BCE477E183", "You have not entered a full name for this person."));
				}
				else if (!IsRightName(Parent.PersonFullName))
				{
					Parent.CPN_PER_PersonInfo.AddMessageError(Res.GetString("BAB95B37-48F3-4FD8-9935-77A26A083F65", "The full name of Stevedores must be in English, Korean, or spaces only. it must not enter numbers or special characters."));
				}
			}
		}

		bool IsRightName(string fullName)
		{
			var name = fullName.Replace(" ", "");

			foreach (var item in name)
			{
				if (!Regex.IsMatch(item.ToString(), @"[a-zA-Z]") && !Regex.IsMatch(item.ToString(), @"[ㄱ-ㅎ가-힣]"))
				{
					return false;
				}
			}
			return true;
		}
	}
}
