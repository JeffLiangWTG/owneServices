using CargoWise.EntityFramework;
using Enterprise.ZArchitecture.Business;
using Res = ZClientEDI.Res;

namespace Enterprise.Client.EDI.MasterFiles.Module
{
	public class MembershipFilterValidation : ModuleTextFilterValidation
	{
		public MembershipFilterValidation(MembershipFilter parent)
			: base(parent)
		{
			this.parent = parent;
		}

		readonly MembershipFilter parent;

		static string FromShouldBeEarlierError => Res.GetString("FDA5D276-F8F5-4348-A97F-D7D302B6FE33", "From date should be the same or earlier than a corresponding To date");
		static string NoOverlapError => Res.GetString("5C4EF023-B13B-4547-B5EB-8B117B0353B7", "There should be an overlap between the specified 'Valid From' and 'Valid To' periods.");

		bool CheckNoOverlap()
		{
			return !parent.ValidFromFrom.IsEmpty && !parent.ValidToTo.IsEmpty && parent.ValidFromFrom > parent.ValidToTo;
		}

		internal void ValidateValidFromFrom()
		{
			ValidateCalculatedProperty(parent.ValidFromFromInfo);
		}

		protected void CheckValidFromFrom()
		{
			if (parent == null || parent.ValidFromFrom.IsEmpty)
			{
				return;
			}

			if (!parent.ValidFromTo.IsEmpty && parent.ValidFromFrom > parent.ValidFromTo)
			{
				parent.ValidFromFromInfo.AddError(FromShouldBeEarlierError);
			}

			if (CheckNoOverlap())
			{
				parent.ValidFromFromInfo.AddError(NoOverlapError);
			}
		}

		internal void ValidateValidFromTo()
		{
			ValidateCalculatedProperty(parent.ValidFromToInfo);
		}

		protected void CheckValidFromTo()
		{
			if (parent == null || parent.ValidFromTo.IsEmpty)
			{
				return;
			}

			if (!parent.ValidFromTo.IsEmpty && parent.ValidFromFrom > parent.ValidFromTo)
			{
				parent.ValidFromToInfo.AddError(FromShouldBeEarlierError);
			}
		}

		internal void ValidateValidToFrom()
		{
			ValidateCalculatedProperty(parent.ValidToFromInfo);
		}

		protected void CheckValidToFrom()
		{
			if (parent == null || parent.ValidToFrom.IsEmpty)
			{
				return;
			}

			if (!parent.ValidToTo.IsEmpty && parent.ValidToFrom > parent.ValidToTo)
			{
				parent.ValidToFromInfo.AddError(FromShouldBeEarlierError);
			}
		}

		internal void ValidateValidToTo()
		{
			ValidateCalculatedProperty(parent.ValidToToInfo);
		}

		protected void CheckValidToTo()
		{
			if (parent == null || parent.ValidToTo.IsEmpty)
			{
				return;
			}

			if (!parent.ValidToTo.IsEmpty && parent.ValidToFrom > parent.ValidToTo)
			{
				parent.ValidToToInfo.AddError(FromShouldBeEarlierError);
			}

			if (CheckNoOverlap())
			{
				parent.ValidToToInfo.AddError(NoOverlapError);
			}
		}

		internal void ValidateMembershipType()
		{
			ValidateCalculatedProperty(parent.MembershipTypeInfo);
		}

		protected void CheckMembershipType()
		{
			if (parent == null)
			{
				return;
			}

			ListValidation.ErrorIfInvalidCode(parent.MembershipTypeInfo);
		}

		internal void ValidateOrganisation()
		{
			ValidateCalculatedProperty(parent.OrganisationInfo);
		}

		protected void CheckOrganisation()
		{
			if (parent == null)
			{
				return;
			}

			ListValidation.ErrorIfInvalidPK(parent.OrganisationInfo);
		}

		public override void ValidateAll()
		{
			base.ValidateAll();
			ValidateMembershipType();
			ValidateOrganisation();
			ValidateValidFromFrom();
			ValidateValidFromTo();
			ValidateValidToFrom();
			ValidateValidToTo();
			ValidateAgreementVersionFrom();
			ValidateAgreementVersionTo();
		}

		internal void ValidateAgreementVersionFrom()
		{
			ValidateCalculatedProperty(parent.AgreementVersionFromInfo);
		}

		internal void ValidateAgreementVersionTo()
		{
			ValidateCalculatedProperty(parent.AgreementVersionToInfo);
		}
	}
}
