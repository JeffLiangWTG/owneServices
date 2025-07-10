using CargoWise.EntityFramework;
using Enterprise.Customs.KR.Messaging;

namespace Enterprise.Customs.KR.Business
{
	public class PIDCusPersonValidation : CusPersonValidation
	{
		public PIDCusPersonValidation(CusPerson parent)
			: base(parent)
		{
		}

		protected override void CheckCPN_PER_Person()
		{
			base.CheckCPN_PER_Person();
			var declaration = Parent.ParentJobDeclaration;
			if (Parent.Person != null && declaration != null)
			{
				foreach (CusPerson person in declaration.Persons)
				{
					if (person.PK != Parent.PK && person.Person?.PK == Parent.Person.PK)
					{
						Parent.CPN_PER_PersonInfo.AddError(Res.GetString("D0ABC6D7-0074-4DA6-8046-0B37F9C25CC9", "You have already entered the person"));
						break;
					}
				}
			}
		}

		const string Oneself = RelationshipCodeList.Codes._00;
		protected override void CheckRelationshipToDeclarant()
		{
			ListValidation.MessageErrorIfInvalidCode(Parent.RelationshipToDeclarantInfo);
			var declaration = Parent.ParentJobDeclaration;
			if (declaration != null && Parent.RelationshipToDeclarant == Oneself)
			{
				foreach (CusPerson person in declaration.Persons)
				{
					if (person.PK != Parent.PK && person.RelationshipToDeclarant == Oneself)
					{
						Parent.RelationshipToDeclarantInfo.AddError(Res.GetString("EBFCE240-2960-4F94-B6A1-89DA7F5506E8", "You have already entered the code"));
						break;
					}
				}
			}
		}
	}
}
