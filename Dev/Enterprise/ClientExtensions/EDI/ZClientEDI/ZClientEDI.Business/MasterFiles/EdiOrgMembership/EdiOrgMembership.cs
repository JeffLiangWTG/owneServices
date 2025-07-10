using System;
using System.Data;
using System.Linq;
using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using CargoWise.Types;
using CargoWiseOne.ResourceStrings;
using Enterprise.ZArchitecture.Core;
using Res = ZClientEDI.Business.Res;

namespace Enterprise.Client.EDI.MasterFiles.Business
{
	public class EdiOrgMembership : AutoEdiOrgMembership
	{
		public EdiOrgMembership(BusinessObjectFactory factory, DataRow row) : base(factory, row)
		{
		}

		[List("Lookups.MembershipTypes")]
		[ResourceStringData("EdiOrgMembership|EOR_MembershipType", Caption = "Membership Type")]
		public override ZString EOR_MembershipType
		{
			get
			{
				LatestMembershipType = base.EOR_MembershipType;
				return base.EOR_MembershipType;
			}
			set
			{
				// Expose the 'EOR_MembershipType' value in a getter to the validation and
				// avoid to invoke the setter in the 'base' too early;
				LatestMembershipType = value.ToString();
				if (EOR_OH_Organisation_ReadOnly)
				{
					base.EOR_MembershipType = value;
					// This will trigger the setter in base and the validation of 'CheckEOR_OH_Organisation'
					EOR_OH_Organisation = ZGuid.Empty;
				}
				// Notice: if put 'EOR_MembershipType' assignment at the beginning this setter,
				// the 'EOR_OH_Organisation' column would not get updated properly within the 'base' class,
				// because the parent variable would be completely overwitten in the 'ZValidation' constructor
				// before updating the value of 'EOR_OH_Organisation' in the 'parent.Row.itemArray[]'.
				base.EOR_MembershipType = value;
			}
		}

		[ResourceStringData("EdiOrgMembership|EOR_ValidFrom", Caption = "Valid From")]
		public override ZDate EOR_ValidFrom { get => base.EOR_ValidFrom; set => base.EOR_ValidFrom = value; }

		[ResourceStringData("EdiOrgMembership|EOR_ValidTo", Caption = "Valid To")]
		public override ZDate EOR_ValidTo { get => base.EOR_ValidTo; set => base.EOR_ValidTo = value; }

		[ResourceStringData("EdiOrgMembership|EOR_AgreementVersion", Caption = "Agreement Version")]
		public override ZDate EOR_AgreementVersion { get => base.EOR_AgreementVersion; set => base.EOR_AgreementVersion = value; }

		[ResourceStringData("EdiOrgMembership|EOR_OH_Organisation", Caption = "Organization")]
		public override ZGuid EOR_OH_Organisation
		{
			get => base.EOR_OH_Organisation;
			set => base.EOR_OH_Organisation = value;
		}

		[ResourceStringData("EdiOrgMembership|EOR_Status", Caption = "Status")]
		public ZString EOR_Status
		{
			get
			{
				if (EOR_ValidTo.IsEmpty && EOR_ValidFrom.IsEmpty)
				{
					return String.Empty;
				}
				ZDate currentDate = ZDateTime.UtcNow.Date;
				return (EOR_ValidTo.IsEmpty || currentDate >= EOR_ValidFrom && currentDate <= EOR_ValidTo)
					? Res.GetString("558fc8b0-621d-4f39-ac0d-041b407212a3", "Active")
					: Res.GetString("f07313ae-a947-4d9b-ab0e-e09c31e2d352", "Inactive");
			}
		}

		// Check if a Membership Type does not require an Organisation;
		bool EOR_OH_Organisation_ReadOnly =>
			!EDIDataRegistry.Instance.OrgMembershipTypes.Value.GetActiveCodeDescriptionPairList().GetAllCodes().Contains(LatestMembershipType);

		public string LatestMembershipType { get; set; }
	}
}

