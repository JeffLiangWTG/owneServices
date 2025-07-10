using CargoWise.Types;
using Enterprise.Customs.Business;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Core;

namespace Enterprise.Customs.JP.Business
{
	public class JPJobDocAddressLookups : JobDocAddressLookups
	{
		public JPJobDocAddressLookups(AutoJobDocAddress parent) : base(parent)
		{
		}

		public override CodeDescriptionPairList GovRegNumTypes
		{
			get
			{
				var parent = Parent;
				var addressType = parent.E2_AddressType;
				var messageType = parent.Parent is JobDeclaration declaration ? declaration.JE_MessageType : ZString.Empty;
				return Factory.GetCachedValue($"Enterprise.Customs.JP.Business.JPJobDocAddressLookups.GovRegNumTypes|{parent.E2_AddressType}|{messageType}", () =>
				{
					var result = new CodeDescriptionPairList();
					var orgCodes = Common.CustomsCodesList.GetJPCustomsCodesList(Factory);
					switch ($"{addressType}{messageType}")
					{
						case $"{DocAddressTypes.Codes.SupplierDocumentaryAddress}{JobMessageTypeList.Codes.Import}":
						case $"{DocAddressTypes.Codes.ImporterDocumentaryAddress}{JobMessageTypeList.Codes.Export}":
						case $"{DocAddressTypes.Codes.ConsignorAddress}{JobMessageTypeList.Codes.Import}":
						case $"{DocAddressTypes.Codes.ConsigneeAddress}{JobMessageTypeList.Codes.Export}":
							result.AddPair(OrgCusCode.JapanCodeTypes.FSB, orgCodes.GetDescriptionFromCode(OrgCusCode.JapanCodeTypes.FSB));
							result.AddPair(OrgCusCode.JapanCodeTypes.CIE, orgCodes.GetDescriptionFromCode(OrgCusCode.JapanCodeTypes.CIE));
							break;
						case $"{DocAddressTypes.Codes.ExternalBroker}{JobMessageTypeList.Codes.Import}":
						case $"{DocAddressTypes.Codes.ExternalBroker}{JobMessageTypeList.Codes.Export}":
						case $"{DocAddressTypes.Codes.InspectionWitness}{JobMessageTypeList.Codes.Import}":
						case $"{DocAddressTypes.Codes.InspectionWitness}{JobMessageTypeList.Codes.Export}":
						case $"{DocAddressTypes.Codes.AirCargoAgent}{JobMessageTypeList.Codes.Import}":
						case $"{DocAddressTypes.Codes.AirCargoAgent}{JobMessageTypeList.Codes.Export}":
							result.AddPair(OrgCusCode.JapanCodeTypes.NUC, orgCodes.GetDescriptionFromCode(OrgCusCode.JapanCodeTypes.NUC));
							break;
						default:
							result.AddPair(OrgCusCode.JapanCodeTypes.LPC, orgCodes.GetDescriptionFromCode(OrgCusCode.JapanCodeTypes.LPC));
							result.AddPair(OrgCusCode.JapanCodeTypes.CIE, orgCodes.GetDescriptionFromCode(OrgCusCode.JapanCodeTypes.CIE));
							result.AddPair(OrgCusCode.JapanCodeTypes.JAS, orgCodes.GetDescriptionFromCode(OrgCusCode.JapanCodeTypes.JAS));
							break;
					}
					
					return result;
				});
			}
		}
	}
}
