using CargoWise.Application;
using Enterprise.Customs.ES.Business.CusTempStorage;
using Enterprise.Integration.Licensing;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Customs.ES.TemporaryStorage.GUI
{
	public class G5V1TemporaryStorageLayout : IPanelLayoutProvider
	{
		public G5V1TemporaryStorageLayout()
		{
			layout = CreateLayout();
		}

		readonly PanelLayout layout;
		PanelLayout IPanelLayoutProvider.Layout => layout;

		PanelLayout CreateLayout()
		{
			var builder = new EU.TemporaryStorage.GUI.TemporaryStorageLayoutBuilder<TemporaryStorageHeader>();
			var commonBag = builder.CommonBag;
			var esBag = G5V1TemporaryStorageUserControlBag.Instance;

			builder.AddControlBag(esBag);

			builder.AddColumn();
			builder.Add(commonBag.CountryCodeFindBox, ControlWidthClass.Medium);
			builder.Add(commonBag.MessageTypeDropEdit, ControlWidthClass.Long);
			builder.Add(esBag.IsSimplifiedCheckBox, ControlWidthClass.Long);
			builder.Add(commonBag.TransportModeDropEdit, ControlWidthClass.Long);
			builder.Add(commonBag.TransportTypeDropEdit, ControlWidthClass.Long);
			builder.Add(commonBag.ArrivalTransportMeansCodeTextBox, ControlWidthClass.Medium);
			builder.Add(commonBag.DeclarantAddressControl, ControlWidthClass.Long);
			builder.Add(commonBag.RepresentativeAddressControl, ControlWidthClass.Long);
			builder.Add(commonBag.SupervisingCustomsOfficeCodeFindBox, ControlWidthClass.Long);
			builder.Add(commonBag.LocationOfGoodsUserControl, ControlWidthClass.Long);
			builder.Add(esBag.DestinationCustomsOfficeCodeFindBox, ControlWidthClass.Long);
			builder.Add(esBag.DestinationLocationOfGoodsUserControl, ControlWidthClass.Long);
			builder.Add(esBag.ManualLocationOfGoodsCodeFindBox, ControlWidthClass.Long);
			builder.Add(esBag.TransportDocumentTypeDropEdit, ControlWidthClass.Long);
			builder.Add(esBag.TransportDocumentTextBox, ControlWidthClass.Long);
			builder.Add(esBag.UnionGoodsCheckBox, ControlWidthClass.Long);

			builder.Add(commonBag.AuthorizationTypeDropEdit, ControlWidthClass.Long);
			builder.Add(commonBag.AuthorizationOwnerGuidFindBox, ControlWidthClass.Long);
			builder.Add(commonBag.AuthorizationNumberCodeFindBox, ControlWidthClass.Long);
			builder.Add(commonBag.HasHouseConsignmentCheckBox, ControlWidthClass.Long);
			builder.Add(esBag.MovementOfContainersOnlyCheckBox, ControlWidthClass.Long);
			builder.Add(commonBag.CusAgentCodeFindBox, ControlWidthClass.Long);
			builder.Add(esBag.CertificateDropEdit, ControlWidthClass.Long);
			builder.Add(esBag.TrainingCheckBox, ControlWidthClass.Long);

			builder.AddColumn();
			builder.Add(esBag.DeclarationDetailsGroupBox, ControlWidthClass.Auto);
			builder.Add(esBag.GuaranteeGroupBox, ControlWidthClass.Auto);
			builder.Add(esBag.DocumentsTabControl, ControlWidthClass.Auto);

			builder.SetVisibility(commonBag.DeclarantAddressControl, h => !h.IsMessageTypeManual, h => h.AMA_MessageTypeInfo);
			builder.SetVisibility(commonBag.RepresentativeAddressControl, h => !h.IsMessageTypeManual, h => h.AMA_MessageTypeInfo);
			builder.SetVisibility(commonBag.SupervisingCustomsOfficeCodeFindBox, h => !h.IsMessageTypeManual, h => h.AMA_MessageTypeInfo);
			builder.SetVisibility(commonBag.LocationOfGoodsUserControl, h => !h.IsMessageTypeManual, h => h.AMA_MessageTypeInfo);
			builder.SetVisibility(esBag.DestinationCustomsOfficeCodeFindBox, h => !h.IsMessageTypeManual, h => h.AMA_MessageTypeInfo);
			builder.SetVisibility(esBag.DestinationLocationOfGoodsUserControl, h => !h.IsMessageTypeManual, h => h.AMA_MessageTypeInfo);
			builder.SetVisibility(esBag.ManualLocationOfGoodsCodeFindBox, h => h.IsMessageTypeManual, h => h.AMA_MessageTypeInfo);
			builder.SetVisibility(commonBag.AuthorizationTypeDropEdit, h => !h.IsMessageTypeManual, h => h.AMA_MessageTypeInfo);
			builder.SetVisibility(commonBag.AuthorizationOwnerGuidFindBox, h => !h.IsMessageTypeManual, h => h.AMA_MessageTypeInfo);
			builder.SetVisibility(commonBag.AuthorizationNumberCodeFindBox, h => !h.IsMessageTypeManual, h => h.AMA_MessageTypeInfo);
			builder.SetVisibility(commonBag.HasHouseConsignmentCheckBox, h => h.IsMessageType_G5E_G5R, h => h.AMA_MessageTypeInfo);
			builder.AddControlBehaviour<ZCheckBox>(commonBag.HasHouseConsignmentCheckBox, UpdateHasHouseConsignmentAlignment);
			builder.SetVisibility(esBag.MovementOfContainersOnlyCheckBox, h => h.IsMessageType_G5E_G5R, h => h.AMA_MessageTypeInfo);
			builder.SetVisibility(commonBag.CusAgentCodeFindBox, h => !h.IsMessageTypeLAM, h => h.AMA_MessageTypeInfo);
			builder.SetVisibility(esBag.CertificateDropEdit, h => !h.IsMessageTypeLAM, h => h.AMA_MessageTypeInfo);
			builder.SetVisibility(esBag.TrainingCheckBox, h => ObjectFactory.Get<IProductRegistration>().IsWiseTechGlobalInternalSystem() && !h.IsMessageTypeLAM, h => h.AMA_MessageTypeInfo);
			builder.SetVisibility(esBag.IsSimplifiedCheckBox, h => h.IsMessageType_G5E_G5R, h => h.AMA_MessageTypeInfo);
			builder.SetVisibility(esBag.DocumentsTabControl, h => !h.IsMessageTypeManual, h => h.AMA_MessageTypeInfo);
			builder.SetVisibility(esBag.GuaranteeGroupBox, h => !h.IsMessageTypeLAM, h => h.AMA_MessageTypeInfo);
			builder.SetVisibility(esBag.UnionGoodsCheckBox, h => h.IsMessageTypeTSM, h => h.AMA_MessageTypeInfo);

			return builder.Build();
		}

		void UpdateHasHouseConsignmentAlignment(ZCheckBox hasHouseConsignmentCheckBox, TemporaryStorageHeader header)
		{
			hasHouseConsignmentCheckBox.CheckAlign = Enterprise.ZArchitecture.GUI.ZContentAlignment.Left;
		}
	}
}
