using System.Collections.Generic;
using System.Linq;
using Enterprise.MasterFiles.Integration;

namespace Enterprise.Client.EDI.MasterFiles.Business
{
	public class EDISalesRelationTypeList : SalesRelationTypeList
	{
		protected EDISalesRelationTypeList()
		{
			AddPair(EDIRelatableActivityTypeList.Codes.Incident, EDIRelatableActivityTypeList.Descriptions.Incident);
		}

		public new static EDISalesRelationTypeList New()
		{
			return new EDISalesRelationTypeList();
		}

		public static void RegisterThisSubTypeOverride()
		{
			OverridableNewDelegate.Value = New;
		}

		public override IEnumerable<string> GetCodes()
		{
			return base.GetCodes().Append(EDIRelatableActivityTypeList.Codes.Incident);
		}
	}
}

