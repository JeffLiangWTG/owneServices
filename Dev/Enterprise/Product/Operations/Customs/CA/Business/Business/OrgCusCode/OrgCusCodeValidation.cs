using Enterprise.MasterFiles.Business;
using static Enterprise.Integration.Customs.CA;

//These are only necessary when ResourceStrings code generation is inactive (e.g. DEBUGFAST)
#pragma warning disable IDE0079
#pragma warning disable IDE0005
using CargoWiseOne.ResourceStrings;
#pragma warning restore IDE0005, IDE0079

namespace Enterprise.Customs.CA.Business
{
	public class OrgCusCodeValidation : MasterFiles.Business.OrgCusCodeValidation, IOrgCusCodeValidation
	{
		public OrgCusCodeValidation(AutoOrgCusCode parent) : base(parent)
		{
		}

		#region OK_CustomsRegNo

		protected override void CheckOK_CustomsRegNo()
		{
			base.CheckOK_CustomsRegNo();

			if (Parent.OK_CodeType == OrgCusCode.CACodeTypes.SafeFoodForCanadiansLicense && !Parent.OK_CustomsRegNo.IsEmpty)
			{
				if (!Parent.OK_CustomsRegNo.IsLettersAndNumbersOnlyOrEmpty)
				{
					Parent.OK_CustomsRegNoInfo.AddMessageError(Res.GetString("592488D5-A585-4DA7-954D-317B65A9DCD4", "The number should be alphanumeric."));
				}
			}
			else if (Parent.OK_CodeType == OrgCusCode.CodeTypes.CarrierCode && !Parent.OK_CustomsRegNo.IsEmpty)
			{
				if (Parent.Header != null && Parent.Header.OH_IsForwarder
					&& (Parent.OK_CustomsRegNo.Length != 4 || Parent.OK_CustomsRegNo[0] != '8' || !Parent.OK_CustomsRegNo.IsLettersAndNumbersOnlyOrEmpty))
				{
					Parent.OK_CustomsRegNoInfo.AddWarning(Res.GetString("da663e4c-e9e4-41f4-b513-ec4db4323ca6", "An incorrectly formatted CCC number has been entered, please use 8XXX, where XXX is alpha-numeric characters only."));
				}
			}
		}

		#endregion

		#region OK_CodeType

		protected override void CheckOK_CodeType()
		{
			base.CheckOK_CodeType();

			if (Parent.OK_CodeType == "AGT")
			{
				Parent.OK_CodeTypeInfo.AddError(Res.GetString("dacdac86-76a9-4475-90ce-c8770d55e8d7", "CA AGT Registration Code is no longer valid. Use CA CCC instead."));
			}
		}

		#endregion
	}
}
