using System.Collections.Generic;
using CargoWise.Common;
using CargoWise.Types;
using Enterprise.Customs.Business;
using Enterprise.MasterFiles.Business;

namespace Enterprise.Client.UPE.Business
{
	public class UPEJobDeclarationRatingAdapter<T> : BaseJobDeclarationRatingAdapter<T>
		where T : UPEJobDeclaration
	{
		public UPEJobDeclarationRatingAdapter(T parent)
			: base(parent)
		{
			Argument.NotNull(parent, "parent");
			this.parent = parent;
		}

		readonly T parent;

		#region IAutoRating / IAutoRatingFreightInfo

		public override ChargeCodeGroupCollection ChargeCodeGroups
		{
			get
			{
				var result = new ChargeCodeGroupCollection();
				result.Add(ChargeCodeGroupList.Codes.Freight);

				return result;
			}
		}

		public override ServiceLevelRatingInformation ServiceLevel
		{
			get
			{
				var infos = new List<ServiceLevelInfo>();

				var result = "";
				string shipmentType = parent.FirstCusHAWB != null ? parent.FirstCusHAWB.ShipmentType : ZString.Empty;
				if (parent.JE_RS_NKServiceLevel == "1" || parent.JE_RS_NKServiceLevel == "21")
				{
					if (shipmentType == ShipmentTypeCodeDescriptionPairList.Codes.Letter)
					{
						result = UPERatingConstants.ServiceLevels.Envelopes;
					}
					else if (shipmentType == ShipmentTypeCodeDescriptionPairList.Codes.Documents)
					{
						result = UPERatingConstants.ServiceLevels.Documents;
					}
					else
					{
						result = UPERatingConstants.ServiceLevels.ExpressPackages;
					}
				}
				else if (parent.JE_RS_NKServiceLevel == "5")
				{
					if (shipmentType == ShipmentTypeCodeDescriptionPairList.Codes.NonDocuments)
					{
						result = UPERatingConstants.ServiceLevels.ExpeditedPackages;
					}
				}
				else if (parent.JE_RS_NKServiceLevel == "28")
				{
					result = UPERatingConstants.ServiceLevels.ExpressSaver;
				}

				infos.Add(new ServiceLevelInfo(result, ServiceLevelType.Client));
				infos.Add(new ServiceLevelInfo(result, ServiceLevelType.Carrier));

				return new ServiceLevelRatingInformation(infos.ToArray());
			}
		}

		#endregion
	}
}

#region IAutoRating / IAutoRatingFreightInfo
#endregion
