using System;
using System.Collections.Generic;
using System.Linq;
using CargoWise.Types;

namespace Enterprise.Customs.ES.Business.Declaration
{
	public class ESGuaranteeCollection : EU.Business.Declaration.GuaranteeForDeclarationCollection
	{
		public ESGuaranteeCollection(JobDeclaration declaration)
				: base(declaration)
		{
		}

		public new ESGuarantee this[int index] => (ESGuarantee)Elements[index];

		public new ESGuarantee AddNew() => (ESGuarantee)base.AddNew();

		protected new ESGuarantee AddNew(Type type) => (ESGuarantee)base.AddNew(type);

		public ZString GetReferenceForType(ZGuid entryInstructionPK, ZString guaranteeType)
		{
			return this.Cast<ESGuarantee>().FirstOrDefault(x => x.EntryInstructionID == entryInstructionPK && x.PW_BondType == guaranteeType)?.PW_BondNumber ?? ZString.Empty;
		}

		public List<ZString> GetGRNReferencesForCharacter(ZGuid entryInstructionPK, char specificCharacter)
		{
			return this.Cast<ESGuarantee>().Where(x => x.EntryInstructionID == entryInstructionPK && x.PW_BondType.IsEmpty && x.PW_BondNumber.Length > 5 && x.PW_BondNumber[4] == specificCharacter).Select(x => x.PW_BondNumber).ToList();
		}
	}
}
