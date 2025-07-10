using System;
using System.ComponentModel;
using CargoWise.Common;
using CargoWise.EntityFramework;
using CargoWise.Types;

namespace Enterprise.Customs.EU.NCTS.Business
{
	public interface INctsAdditionalInfoCollection<out T> : Customs.Business.ICusSupportingInfoCollection<T>, IBindingList
		where T : NctsAdditionalInfo
	{
		new T this[int index] { get; }
		new T AddNew();
		void RefreshBinding();
	}

	public class NctsAdditionalInfoCollection<T> : Customs.Business.CusSupportingInfoCollection<T>, INctsAdditionalInfoCollection<T>
		where T : NctsAdditionalInfo
	{
		public NctsAdditionalInfoCollection(Integration.Customs.ICusSupportingInfoTypeSupporter parent)
			: base((BusinessObject)parent, Common.EU.CusSupportingInfoTypeList.Codes.AdditionalInfo)
		{
			cusSupportingInfoTypeSupporter = parent;
		}
		readonly Integration.Customs.ICusSupportingInfoTypeSupporter cusSupportingInfoTypeSupporter;

		protected override void OnRemoved(BusinessObject bizO)
		{
			base.OnRemoved(bizO);
			if (bizO is NctsAdditionalInfo additionalInfo
				&& additionalInfo.Header is NctsHeader header
				&& header.IsPhase5Departure)
			{
				header.Consignee.Validation.ValidateOrganisationPK();
				header.Bills.ForEach(x => x.Consignee.Validation.ValidateOrganisationPK());
			}
		}

		public override Type GetTypeOfElementsFromPK(ZGuid pK)
		{
			if (typeOfElements == null)
			{
				typeOfElements = typeof(T);
				if (typeOfElements == typeof(NctsAdditionalInfo))
				{
					if (!cusSupportingInfoTypeSupporter.GetCusSupportingInfoTypes().TryGetValue(Common.EU.CusSupportingInfoTypeList.Codes.AdditionalInfo, out typeOfElements))
					{
						throw new NotImplementedException($"{cusSupportingInfoTypeSupporter.GetType().FullName}.GetCusSupportingInfoTypes() is missing support for '{Common.EU.CusSupportingInfoTypeList.Codes.AdditionalInfo}'.");
					}
				}
			}
			return typeOfElements;
		}
		Type typeOfElements;
	}
}
