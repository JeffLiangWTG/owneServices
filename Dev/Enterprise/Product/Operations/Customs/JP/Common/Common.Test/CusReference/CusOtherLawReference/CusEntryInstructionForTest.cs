using System.Collections.Generic;
using System.Data;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.Business;

namespace Enterprise.Customs.JP.Common.Testing
{
	public class CusEntryInstructionForTest : CusEntryInstruction, ICusOtherLawReferenceParent
	{
		public CusEntryInstructionForTest(BusinessObjectFactory factory, DataRow row) : base(factory, row)
		{
		}

		ZBool ICusOtherLawReferenceParent.IsOtherLawReferenceRequired => CEI_Style == "A";

		ZString ICusOtherLawReferenceParent.MessageType => CEI_SubStyle;

		public IEnumerable<ZString> GetTariffAttributesByKey(ZString key)
		{
			yield return "CO3";
			yield return "CO4";
		}
	}
}
