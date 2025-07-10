using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using CargoWise.Common;
using CargoWise.Customs.FR.MessageDefinitions.DeltaIE.Interfaces;
using Enterprise.Customs.FR.Business.Declaration;
using AdditionalInfo = Enterprise.Customs.FR.Business.Declaration.AdditionalInfo;

namespace Enterprise.Customs.FR.Business.MessagesWrappers.DeltaIE
{
	public class ConsignmentWrapper : IConsignment
	{
		ConsignmentWrapper(CusEntryHeader entry)
		{
			this.entry = Argument.NotNull(entry, nameof(entry));
			this.declaration = Argument.NotNull(entry.Declaration, nameof(declaration));
		}
		readonly CusEntryHeader entry;
		readonly JobDeclaration declaration;

		public static ConsignmentWrapper New(CusEntryHeader entry) => entry?.Declaration != null ? new ConsignmentWrapper(entry) : null;

		public IActiveBorderTransportMeans ActiveBorderTransportMeans => activeBorderTransportMeans ?? (activeBorderTransportMeans = ActiveBorderTransportMeansWrapper.New(declaration));
		IActiveBorderTransportMeans activeBorderTransportMeans;

		public IArrivalTransportMeans ArrivalTransportMeans => arrivalTransportMeans ?? (arrivalTransportMeans = ArrivalTransportMeansWrapper.New(declaration));
		IArrivalTransportMeans arrivalTransportMeans;

		public string ContainerIndicator => declaration.IsContainerizedAndHasContainer ? "1" : "0";

		public double GrossMass => (double)declaration.JE_TotalWeight;

		public string InlandModeOfTransport => inlandModeOfTransport ?? (inlandModeOfTransport = declaration.TransportModeTranslator.TranslateToWCOCode(declaration.JE_TransportModeInland, true));
		string inlandModeOfTransport;

		public ILocationOfGoods LocationOfGoods => locationOfGoods ?? (locationOfGoods = LocationOfGoodsWrapper.New(entry.EntryInstruction?.GoodsLocation));
		ILocationOfGoods locationOfGoods;

		public string ModeOfTransportAtTheBorder => modeOfTransportAtTheBorder ?? (modeOfTransportAtTheBorder = declaration.TransportModeTranslator.TranslateToWCOCode(declaration.JE_TransportMode, true));
		string modeOfTransportAtTheBorder;

		public string ReferenceNumberUCR => referenceNumberUCR ?? (referenceNumberUCR = declaration.JE_UCR);
		string referenceNumberUCR;

		public ICollection<ITransportDocument> TransportDocument => transportDocument ?? (transportDocument = GetTransportDocumentCollection());
		ICollection<ITransportDocument> transportDocument;

		ICollection<ITransportDocument> GetTransportDocumentCollection()
		{
			var listOfAdditionalInfo = entry.AdditionalInfos
				.Where(x => x.IsATransportDocument)
				.Cast<AdditionalInfo>();

			var listOfTransportDocumentWrapper = new Collection<ITransportDocument>();

			foreach (var additionalInfo in listOfAdditionalInfo)
			{
				listOfTransportDocumentWrapper.Add(TransportDocumentWrapper.New(additionalInfo));
			}

			return listOfTransportDocumentWrapper.Any() ? listOfTransportDocumentWrapper : null;
		}

		public ICollection<ITransportEquipment> TransportEquipment => transportEquipment ?? (transportEquipment = GetTransportEquipment());
		ICollection<ITransportEquipment> transportEquipment;

		ICollection<ITransportEquipment> GetTransportEquipment()
		{
			var listOfTransportEquipmentWrapper = new Collection<ITransportEquipment>();

			var dict = new Dictionary<string, List<string>>();

			foreach (var entryLine in entry.MergedLines.Where(x => x.PackagingDetails.Any(packagingDetail => packagingDetail?.Package != null && !packagingDetail.Package.CW_ContainerNoOrEquipmentNo.IsEmpty)))
			{
				var lineNumber = entryLine.CL_LineNumber.ToString();

				foreach (var packagingDetail in entryLine.PackagingDetails.Where(packagingDetail => packagingDetail?.Package != null && !packagingDetail.Package.CW_ContainerNoOrEquipmentNo.IsEmpty))
				{
					var number = packagingDetail.Package.CW_ContainerNoOrEquipmentNo;

					List<string> list;

					if (!dict.TryGetValue(number, out list))
					{
						list = new List<string>();
						dict.Add(number, list);
					}

					if (!list.Contains(lineNumber))
					{
						list.Add(lineNumber);
					}
				}
			}

			dict.ForEach(x => listOfTransportEquipmentWrapper.Add(TransportEquipmentWrapper.New(x.Key, x.Value)));

			return listOfTransportEquipmentWrapper;
		}
	}
}
