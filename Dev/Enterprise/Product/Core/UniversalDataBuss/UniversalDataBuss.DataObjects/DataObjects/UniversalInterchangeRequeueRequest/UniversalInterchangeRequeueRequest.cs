using System.Collections.Generic;
using CargoWise.ComponentModel;
using CargoWise.Types;
using Enterprise.UniversalDataBuss.DataObjects.Core;
using Enterprise.UniversalDataBuss.Integration;

namespace Enterprise.UniversalDataBuss.DataObjects.Universal
{
	[XsdSchema("UniversalInterchangeRequeueRequest.xsd"), RootElement("UniversalInterchangeRequeueRequest")]
	public partial class InterchangeRequeueRequest : TopLevelDataObject
	{
		[NamespaceDependent(UniversalXmlInfo.Namespace_2011_11, typeof(_2011_11.DataContext))]
		[NamespaceDependent(UniversalXmlInfo.Namespace_2012_11, typeof(_2012_11.DataContext))]
		[ReferenceProperty]
		public override IDataContextDataObject DataContext { get; set; }
		public List<InterchangeRequeueRequestFilter> FilterCollection { get; set; }
	}

	[XsdSchema(Placement.Inner)]
	public class InterchangeRequeueRequestFilter : IDataObject
	{
		public const string FilterTypeCreateDateUTCFrom = "CreateDateUTCFrom";
		public const string FilterTypeCreateDateUTCTo = "CreateDateUTCTo";
		public const string FilterTypeApplicationCode = "ApplicationCode";
		public const string FilterTypeInterchangeNumberFrom = "InterchangeNumberFrom";
		public const string FilterTypeInterchangeNumberTo = "InterchangeNumberTo";
		public const string FilterTypeSenderCode = "SenderCode";
		public const string FilterTypeRecipientCode = "RecipientCode";
		public const string FilterTypeBranchCode = "BranchCode";

		public static IEnumerable<string> AllSupportedFilterTypes
		{
			get
			{
				return new[]
				{
					FilterTypeCreateDateUTCFrom,
					FilterTypeCreateDateUTCTo,
					FilterTypeApplicationCode,
					FilterTypeInterchangeNumberFrom,
					FilterTypeInterchangeNumberTo,
					FilterTypeSenderCode,
					FilterTypeRecipientCode,
					FilterTypeBranchCode
				};
			}
		}

		[MaxLength(255)]
		public ZString? Type { set; get; }

		[MaxLength(255)]
		public ZString? Value { set; get; }
	}
}
