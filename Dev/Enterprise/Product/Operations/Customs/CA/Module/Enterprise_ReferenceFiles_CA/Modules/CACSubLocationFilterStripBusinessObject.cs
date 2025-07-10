using System.Collections.Generic;
using CargoWise.Types;
using Enterprise.Customs.CA.Business;
using Enterprise.Customs.Universal.Module;

namespace Enterprise.Customs.CA.Module
{
	public class CACSubLocationFilterStripBusinessObject : ZZRefCusCodeListWrapperFilterStripBusinessObject
	{
		protected override Dictionary<string, string> AttributeFilters
		{
			get
			{
				return new Dictionary<string, string>
				{
					{ CityCaption, UniversalReferenceConstants.RefCusCodeListAttributes.Names.City },
					{ PortCaption, UniversalReferenceConstants.RefCusCodeListAttributes.Names.Port },
					{ TypeCaption, UniversalReferenceConstants.RefCusCodeListAttributes.Names.Type }
				};
			}
		}

		#region SuppressResourceStringsCheckRegion 

		protected override ZString CodeCaption => "Sub-Location Code";
		protected override ZString DescriptionCaption => "Warehouse Name";

		const string CityCaption = "City";
		const string PortCaption = "Port";
		const string TypeCaption = "Type";

		#endregion
	}
}
