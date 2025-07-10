using CargoWise.Customs.BR.MessageDefinitions.ImportLicense.Incoming;
using CargoWise.EntityFramework;
using CargoWise.Types;

namespace Enterprise.Customs.BR.Business
{
	public class ImportLicenseLoadingObjectNcmDetailsCollection : NonPersistentBusinessObjectCollection<ImportLicenseLoadingObjectNcmDetails>
	{
		public ImportLicenseLoadingObjectNcmDetailsCollection(ImportLicenseLoadingObject parent)
			: base(parent.Factory)
		{
		}

		protected override BusinessObject CreateNonPersistentBusinessObject()
		{
			return new ImportLicenseLoadingObjectNcmDetails(Factory);
		}

		public void AddNewItems(listadetalhencmtypeDetalhencmitemdrawback[] items)
		{
			if (items != null)
			{
				foreach (var item in items)
				{
					var newElement = AddNew();
					if (ZShort.TryParse(item.numerosequencialproduto ?? ZString.Empty, out var result))
					{
						newElement.SequencialProductNumber = result;
					}
					newElement.ComercialMeasureUnitName = item.nomeunidademedidacomercializada ?? ZString.Empty;
					newElement.NetWeight = ImportLicenseLoadingObjectParent.ConvertStringToDecimal(item.pesoliquidototal ?? ZString.Empty);
					newElement.ComercialMerchandiseQuantity = ImportLicenseLoadingObjectParent.ConvertStringToDecimal(item.qtdmercadoriaunidadecomercializada ?? ZString.Empty);
					newElement.StatisticMerchandiseQuantity = ImportLicenseLoadingObjectParent.ConvertStringToDecimal(item.qtdmercadoriaunidadeestatistica ?? ZString.Empty);
					newElement.ShipmentTotalValue = ImportLicenseLoadingObjectParent.ConvertStringToDecimal(item.valortotallocalembarque ?? ZString.Empty);
					newElement.ProductDescription = item.descricaoproduto ?? ZString.Empty;
				}
			}
		}
	}
}
