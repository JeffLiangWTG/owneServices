using System.ComponentModel;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Core;
using Enterprise.Environment;
using Enterprise.Freight.Business;
using Enterprise.Freight.CFS.Business;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Core;
using Res = DocumentWrappers.Res;

namespace Enterprise.DocumentWrappers
{
	public class DocLoadListConsol : DocBaseConsol, Integration.DocumentWrappers.IDocLoadListConsol
	{
		DocLoadListConsol(CFSLoadListConsol loadListConsol, BusinessObjectFactory factoryToWrap)
			: base(loadListConsol, factoryToWrap)
		{
		}

		public static DocLoadListConsol New(CFSLoadListConsol loadListConsol, BusinessObjectFactory factoryToWrap)
		{
			return (loadListConsol != null) ? new DocLoadListConsol(loadListConsol, factoryToWrap) : null;
		}

		public static DocLoadListConsol New(DocumentCommonConsol documentLoadListConsol, BusinessObjectFactory factoryToWrap)
		{
			DocLoadListConsol wrapper = null;
			if (documentLoadListConsol != null && documentLoadListConsol.Consol != null)
			{
				wrapper = New((CFSLoadListConsol)documentLoadListConsol.Consol, factoryToWrap);
				wrapper.SetFromDocumentCommonConsol(documentLoadListConsol);
			}
			return wrapper;
		}

		public override string ToString()
		{
			return base.ConsolNumber;
		}

		CFSLoadListConsol LoadListConsol
		{
			get { return (CFSLoadListConsol)WrappedObject; }
		}

		public ZString Context
		{
			get { return "CFSCONSOL"; }
		}
		public DocOrganisation CurrentForwarder
		{
			get { return DocOrganisation.New(LoadListConsol.Forwarder, Factory); }
		}

		public ZString VoyageNo
		{
			get { return LoadListConsol.JK_JX_JV_VoyageFlight; }
		}

		public override ZString ContainerInfoForInvoice
		{
			get
			{
				ZString result = ZString.Empty;

				foreach (DocPackUnpackContainerRego currentContainer in ContainerRegos)
				{
					result += currentContainer.ContainerJobID + "/" + currentContainer.ContainerNumber + "/" + currentContainer.ContainerMode;

					DocRefContainer containerType = currentContainer.Container;
					if (containerType != null)
					{
						result += "/" + containerType.Code + ", ";
					}
					else
					{
						result += "/NA,";
					}
				}

				result = result.Trim().TrimEndIncludingWhiteSpace(',');
				return result;
			}
		}

		public ZString ContainersNumbers
		{
			get
			{
				ZString result = ZString.Empty;

				foreach (DocPackUnpackContainerRego currentContainer in ContainerRegos)
				{
					result += currentContainer.ContainerNumber + "; ";
				}

				result = result.Trim();
				return result;
			}
		}

		public ZString LoadListInstructions
		{
			get { return GetNotes(PredefinedNoteTypes.Instance.LoadListInstructions.Description, LoadListConsol); }
		}

		public ZString HandlingInstructions
		{
			get { return GetNotes(PredefinedNoteTypes.Instance.HandlingInstructions.Description, LoadListConsol); }
		}

		public ZString CartageInstructions
		{
			get { return PickupOrDeliveryCartageInstructions(); }
		}

		public ZDecimal LoadListTotalPackLineWeight
		{
			get
			{
				string unit = LoadListTotalPackLineWeightUnit;

				ZDecimal result = 0m;
				foreach (DocLoadListPackLine packline in PackLinesForLoadList)
				{
					result += Constants.Weight.ConvertSafe(packline.Weight, packline.WeightUQ, unit);
				}

				return result;
			}
		}

		public ZString LoadListTotalPackLineWeightUnit
		{
			get { return Env.Registry.FreightWeightUnit; }
		}

		public ZDecimal LoadListTotalPackLineVolume
		{
			get
			{
				string unit = LoadListTotalPackLineVolumeUnit;

				ZDecimal result = 0m;
				foreach (DocLoadListPackLine packline in PackLinesForLoadList)
				{
					result += Constants.Volume.ConvertSafe(packline.Volume, packline.VolumeUQ, unit);
				}

				return result;
			}
		}

		public ZString LoadListTotalPackLineVolumeUnit
		{
			get { return Env.Registry.FreightVolumeUnit; }
		}

		#region Collections
		public DocShipmentCollection ShipmentReceivals
		{
			get
			{
				DocShipmentCollection shipments = new DocShipmentCollection(LoadListConsol.Factory);
				foreach (CFSShipment shipment in LoadListConsol.Shipments)
				{
					shipments.Add(DocShipment.New(shipment, Factory));
				}
				return shipments;
			}
		}

		public DocShipmentCollection ShipmentsForPrintOnOtherDocs
		{
			get { return ShipmentReceivals; }
		}

		protected DocPackLinesCollection fContainersPackLine;
		public DocPackLinesCollection ContainersPackLine
		{
			get
			{
				if (fContainersPackLine == null)
				{
					fContainersPackLine = new DocPackLinesCollection(LoadListConsol.Factory);
					foreach (CFSContainer container in LoadListConsol.Containers)
					{
						foreach (CFSPackLine pack in container.PackLines)
						{
							DocPackLines line = DocPackLines.New(pack, Factory);
							line.ContainerForLoadList = DocContainer.New(container, Factory);
							fContainersPackLine.Add(line);
						}
					}
				}
				return fContainersPackLine;
			}
		}

		protected DocPackLinesCollection UnAllocatedPackLines
		{
			get
			{
				DocPackLinesCollection result = new DocPackLinesCollection(LoadListConsol.Factory);
				if (LoadListConsol.Schedule != null && LoadListConsol.Schedule.Voyage != null)
				{
					UnAllocatedPackLinesView allUnpackedPacks = LoadListConsol.UnAllocatedPackLines;
					allUnpackedPacks.ShowOnlyReceived = ZBool.False;
					foreach (PackLine unpacked in allUnpackedPacks)
					{
						result.Add(DocPackLines.New(unpacked, Factory));
					}
				}
				return result;
			}
		}

		public DocLoadListPackLineCollection PackLines
		{
			get
			{
				DocLoadListPackLineCollection result = new DocLoadListPackLineCollection(LoadListConsol.Factory);
				DocLoadListPackLineCollection containersPackLineCollection = new DocLoadListPackLineCollection(this, ContainersPackLine, LoadListConsol.Factory);
				DocLoadListPackLineCollection unAllocatedPackLinesCollection = new DocLoadListPackLineCollection(this, UnAllocatedPackLines, LoadListConsol.Factory);

				result.AddRange(containersPackLineCollection);
				result.AddRange(unAllocatedPackLinesCollection);
				result.SortOnShipmentInterimReceipt();
				return result;
			}
		}

		public DocLoadListPackLineCollection PackLinesForLoadList
		{
			get
			{
				DocLoadListPackLineCollection result = new DocLoadListPackLineCollection(LoadListConsol.Factory);
				bool mergeSimilarPackLines = !ReportName.Trim().EndsWith((NoResString)"Detailed", System.StringComparison.OrdinalIgnoreCase);

				if (IncludeAllShipments || IncludePacked)
				{
					DocLoadListPackLineCollection containersPackLineCollection = new DocLoadListPackLineCollection(this, ContainersPackLine, mergeSimilarPackLines, LoadListConsol.Factory);
					result.AddRange(containersPackLineCollection);
				}
				if (IncludeAllShipments || IncludeUnPacked)
				{
					DocLoadListPackLineCollection unAllocatedPackLinesCollection = new DocLoadListPackLineCollection(this, UnAllocatedPackLines, mergeSimilarPackLines, LoadListConsol.Factory);
					result.AddRange(unAllocatedPackLinesCollection);
				}

				if (mergeSimilarPackLines)
				{
					result.SortOnShipmentInterimReceipt();
				}
				else
				{
					result.SortOnPackingOrderShipmentInterimReceipt();
				}

				ZString consignorAndConsignee;
				foreach (DocLoadListPackLine docPackLine in result)
				{
					if (docPackLine.Shipment != null)
					{
						consignorAndConsignee = (IncludeConsignor) ? docPackLine.Shipment.ConsignorAsString : ZString.Empty;
						consignorAndConsignee += (IncludeConsignor && IncludeConsignee) ? (ZString)System.Environment.NewLine : ZString.Empty;
						consignorAndConsignee += (IncludeConsignee) ? docPackLine.Shipment.ConsigneeAsString : ZString.Empty;
						docPackLine.ConsignorAndConsigneeForLoadList = consignorAndConsignee;
						if (IncludeCustomsBroker)
						{
							if (IsImportDocument)
							{
								docPackLine.CustomsBrokerForLoadList = (docPackLine.Shipment.ImportBroker != null) ? Res.GetString("c74986a7-9926-4ad7-8eb9-8d6dc261f1a3", "Broker: {0}", docPackLine.Shipment.ImportBroker.Name) : "";
							}
							else
							{
								docPackLine.CustomsBrokerForLoadList = (docPackLine.Shipment.ExportBroker != null) ? Res.GetString("6ec8c994-49c2-4d59-b651-46de5dfde3d7", "Broker: {0}", docPackLine.Shipment.ExportBroker.Name) : "";
							}
						}
					}

					docPackLine.ContainerTotalManifestWeightUnit = WeightUnit;
					docPackLine.ContainerTotalManifestWeight = Core.Constants.Weight.ConvertSafe(docPackLine.Weight, docPackLine.WeightUQ, docPackLine.ContainerTotalManifestWeightUnit);

					docPackLine.ContainerTotalManifestVolumeUnit = VolumeUnit;
					docPackLine.ContainerTotalManifestVolume = Core.Constants.Volume.ConvertSafe(docPackLine.Volume, docPackLine.VolumeUQ, docPackLine.ContainerTotalManifestVolumeUnit);
				}
				return result;
			}
		}

		public DocPackUnpackContainerRegoCollection ContainerRegos
		{
			get
			{
				DocPackUnpackContainerRegoCollection collection = new DocPackUnpackContainerRegoCollection(Factory);
				foreach (CFSContainer container in LoadListConsol.Containers)
				{
					collection.Add(DocPackUnpackContainerRego.New(container, Factory));
				}

				collection.Sort("ContainerNumber", ListSortDirection.Ascending);
				return collection;
			}
		}

		public DocPackLocationForDocumentCollection WarehouseLocations
		{
			get
			{
				DocPackLocationForDocumentCollection collection = new DocPackLocationForDocumentCollection(Factory);
				foreach (CommonShipment shipment in LoadListConsol.Shipments)
				{
					foreach (PackLocationForDocument location in shipment.PackLocations)
					{
						collection.Add(DocPackLocationForDocument.New(location, Factory));
					}
				}

				collection.SortOnWarehouseLocation();
				return collection;
			}
		}

		#endregion
	}
}
