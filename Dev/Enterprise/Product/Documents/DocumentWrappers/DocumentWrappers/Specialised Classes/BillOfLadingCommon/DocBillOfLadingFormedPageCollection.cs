using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Accounting.Registry.Business;
using Enterprise.Core;
using Enterprise.DocumentEngine.MacroValueProviders.Utilities;
using Enterprise.DocumentWrappers.FormatTables;
using Enterprise.DocumentWrappers.GenericWrappers;
using Enterprise.Environment;
using Enterprise.Integration.ZArchitecture;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Schema;
using Res = DocumentWrappers.Res;

namespace Enterprise.DocumentWrappers
{
	public class DocBillOfLadingFormedPageCollection : NonPersistentBusinessObjectCollection<DocBillOfLadingFormedPage>
	{
		public DocBillOfLadingFormedPageCollection(DocShipment shipmentWrapper, IFormedPagesSupporter formedPagesSupporter, BusinessObjectFactory factory)
			: base(factory)
		{
			this.shipmentWrapper = shipmentWrapper;
			this.formedPagesSupporter = formedPagesSupporter;
			EnsureSectionsGenerated();
		}

		readonly DocShipment shipmentWrapper;
		readonly IFormedPagesSupporter formedPagesSupporter;

		ZString[] followOnSection;

		#region Overrides

		protected override void OnAdded(BusinessObject bizOAdded)
		{
			base.OnAdded(bizOAdded);
			((DocBillOfLadingFormedPage)bizOAdded).PageNo = Count;
		}

		protected override BusinessObject CreateNonPersistentBusinessObject()
		{
			throw new NotSupportedException();
		}

		protected override bool AllowNewCore
		{
			get { return false; }
		}

		#endregion

		#region Generation

		internal void EnsureSectionsGenerated()
		{
			if (shipmentWrapper != null && !sectionsGenerated)
			{
				sectionsGenerated = true;
				GenerateSections();
			}
		}
		bool sectionsGenerated;

		void GenerateSections()
		{
			if (shipmentWrapper.UseMultiPage)
			{
				GenerateSections_MultiPage();
			}
			else
			{
				GenerateSections_FollowOn();
			}
		}

		void GenerateSections_MultiPage()
		{
			SectionSetterPair[] pairs = GetSectionSetterPairs();

			try
			{
				bool isPageEmpty;

				do
				{
					isPageEmpty = true;

					DocBillOfLadingFormedPage page = new DocBillOfLadingFormedPage();

					foreach (SectionSetterPair pair in pairs)
					{
						List<string> list;

						if ((list = pair.Section.ReadPage()) != null)
						{
							isPageEmpty = false;
							pair.Setter(page, string.Join("\r\n", list.ToArray()));
						}
					}

					if (!isPageEmpty || this.Count == 0)
					{
						this.Add(page);
					}
				}
				while (!isPageEmpty);

				followOnSection = Array.Empty<ZString>();
			}
			finally
			{
				for (int i = 0; i < pairs.Length; i++)
				{
					pairs[i].Section.Dispose();
				}
			}
		}

		void GenerateSections_FollowOn()
		{
			SectionSetterPair[] pairs = GetSectionSetterPairs();

			try
			{
				DocBillOfLadingFormedPage page = new DocBillOfLadingFormedPage();

				foreach (SectionSetterPair pair in pairs)
				{
					List<string> list;

					if ((list = pair.Section.ReadPage()) != null)
					{
						pair.Setter(page, string.Join("\r\n", list.ToArray()));
					}
				}

				this.Add(page);

				bool needsSpacer = false;

				List<ZString> followOnList = new List<ZString>();

				foreach (SectionSetterPair pair in pairs)
				{
					List<string> list;

					if ((list = pair.Section.ReadFollowOn()) != null && list.Count > 0)
					{
						if (needsSpacer)
						{
							followOnList.Add("");
						}
						else
						{
							needsSpacer = true;
						}

						foreach (string line in list)
						{
							followOnList.Add(new ZString(line));
						}
					}
				}

				followOnSection = followOnList.ToArray();
			}
			finally
			{
				for (int i = 0; i < pairs.Length; i++)
				{
					pairs[i].Section.Dispose();
				}
			}
		}

		public ZString[] FollowOnSection
		{
			get
			{
				return followOnSection;
			}
		}

		public ZBool HasFollowOnSection
		{
			get
			{
				return FollowOnSection.Length > 0;
			}
		}

		public ZString ShipperLoadAndCount
		{
			get
			{
				return shipmentWrapper.ContainerModeIndex > 0 && formedPagesSupporter.Containers.Cast<DocFormedPagesContainer>().Any(i => i.ContainerMode == Constants.ContainerModes.FCL && (i.DeliveryMode.Contains("*") || i.DeliveryMode.StartsWith("DOOR")))
					? (NoResString)"* Shipper Load and Count" : "";
			}
		}

		public MoneyWrapper PrepaidChargesTotal
		{
			get
			{
				if (prepaidChargesTotal == null)
				{
					CalculateChargeTotals();
				}

				return prepaidChargesTotal;
			}
		}
		MoneyWrapper prepaidChargesTotal;

		public MoneyWrapper CollectChargesTotal
		{
			get
			{
				if (collectChargesTotal == null)
				{
					CalculateChargeTotals();
				}

				return collectChargesTotal;
			}
		}
		MoneyWrapper collectChargesTotal;

		public MoneyWrapper ChargesTotal
		{
			get
			{
				if (chargesTotal == null)
				{
					CalculateChargeTotals();
				}

				return chargesTotal;
			}
		}
		MoneyWrapper chargesTotal;

		void CalculateChargeTotals()
		{
			var collectTotal = 0m;
			var prepaidTotal = 0m;
			var allTotal = 0m;

			foreach (DocJobCharge charge in formedPagesSupporter.AllCharges.Cast<DocJobCharge>().Where(charge => IsNotProfitShareCharge(charge)))
			{
				if (IsCollect(charge))
				{
					if (shipmentWrapper.ChargesDisplay == ChargesDisplayTypes.AllCharges
						|| shipmentWrapper.ChargesDisplay == ChargesDisplayTypes.CollectCharges
						|| shipmentWrapper.ChargesDisplay == ChargesDisplayTypes.OriginalAsAgreedCopyWithCollectCharges
						|| shipmentWrapper.ChargesDisplay == ChargesDisplayTypes.OriginalAsAgreedCopyWithPrepaidAndCollectCharges)
					{
						collectTotal += charge.LocalSellAmount;
						allTotal += charge.LocalSellAmount;
					}
				}
				else if (shipmentWrapper.ChargesDisplay == ChargesDisplayTypes.AllCharges
					|| shipmentWrapper.ChargesDisplay == ChargesDisplayTypes.PrepaidCharges
					|| shipmentWrapper.ChargesDisplay == ChargesDisplayTypes.OriginalAsAgreedCopyWithPrepaidCharges
					|| shipmentWrapper.ChargesDisplay == ChargesDisplayTypes.OriginalAsAgreedCopyWithPrepaidAndCollectCharges)
				{
					prepaidTotal += charge.LocalSellAmount;
					allTotal += charge.LocalSellAmount;
				}
			}

			prepaidChargesTotal = new MoneyWrapper(new Money(prepaidTotal, GlbCompany.CurrentCompany.LocalCurrency), Factory);
			collectChargesTotal = new MoneyWrapper(new Money(collectTotal, GlbCompany.CurrentCompany.LocalCurrency), Factory);
			chargesTotal = new MoneyWrapper(new Money(allTotal, GlbCompany.CurrentCompany.LocalCurrency), Factory);
		}

		#endregion

		#region FormatSections

		FormatSection NewMainSection()
		{
			List<IFormatSectionComponent> list = new List<IFormatSectionComponent>();
			list.Add(NewDetailsTable());

			if (shipmentWrapper.IncludeContainersInMarksAndNumbersSection)
			{
				list.Add(NewContainersTable());
			}

			if (shipmentWrapper.IncludeRORInMarksAndNumbersSection)
			{
				list.Add(NewTopLevelPacksTable());
			}

			if (shipmentWrapper.IncludeBOLClauseSectionInMarksAndNumbersSection)
			{
				list.Add(NewBOLClauseTable());
			}

			if (shipmentWrapper.IncludeExtraSectionInMarksAndNumbersSection)
			{
				list.Add(NewExtraTable());
			}

			return new FormatSection(shipmentWrapper.MarksAndNumbsAndDescHeight, list);
		}

		FormatSection NewContainersSection()
		{
			if (shipmentWrapper.IncludeContainersInMarksAndNumbersSection)
			{
				return new FormatSection(shipmentWrapper.NoOfContainerRows, Array.Empty<IFormatSectionComponent>());
			}
			else
			{
				return new FormatSection(shipmentWrapper.NoOfContainerRows, new IFormatSectionComponent[] { NewContainersTable() });
			}
		}

		FormatSection NewBOLClauseSection()
		{
			if (shipmentWrapper.IncludeBOLClauseSectionInMarksAndNumbersSection || shipmentWrapper.IncludeBOLClauseInGoodsDescription)
			{
				return new FormatSection(shipmentWrapper.NumberOfBOLClauseRows, Array.Empty<IFormatSectionComponent>());
			}
			else
			{
				return new FormatSection(shipmentWrapper.NumberOfBOLClauseRows, new IFormatSectionComponent[] { NewBOLClauseTable() });
			}
		}

		FormatSection NewRORSection()
		{
			if (shipmentWrapper.IncludeRORInMarksAndNumbersSection)
			{
				return new FormatSection(shipmentWrapper.NumberOfRORRows, Array.Empty<IFormatSectionComponent>());
			}
			else
			{
				return new FormatSection(shipmentWrapper.NumberOfRORRows, new IFormatSectionComponent[] { NewTopLevelPacksTable() });
			}
		}

		FormatSection NewChargesSection()
		{
			return new FormatSection(shipmentWrapper.NoOfCollectChargesRows, new IFormatSectionComponent[] { NewChargesTable() });
		}

		FormatSection NewExtraSection()
		{
			if (shipmentWrapper.IncludeExtraSectionInMarksAndNumbersSection)
			{
				return new FormatSection(shipmentWrapper.NoOfExtraSectionRows, Array.Empty<IFormatSectionComponent>());
			}
			else
			{
				return new FormatSection(shipmentWrapper.NoOfExtraSectionRows, new IFormatSectionComponent[] { NewExtraTable() });
			}
		}

		#endregion

		#region FormatTables

		internal IFormatSectionComponent NewDetailsTable()
		{
			List<string> goodsDescription = new List<string>();
			List<string> marksAndNumbers = new List<string>();
			List<string> packageCount = new List<string>();
			List<string> weight = new List<string>();
			List<string> volume = new List<string>();

			for (int i = 0; i < formedPagesSupporter.Shipments.Count; i++)
			{
				if (i > 0)
				{
					goodsDescription.Add(new ZString('-', shipmentWrapper.GoodsDescWidth));
					marksAndNumbers.Add(new ZString('-', shipmentWrapper.MarksAndNumbersWidth));
					packageCount.Add(new ZString('-', shipmentWrapper.PackagesWidth));
					weight.Add(new ZString('-', shipmentWrapper.GrossWeightWidth));
					volume.Add(new ZString('-', shipmentWrapper.VolumeMeasurementWidth));
				}

				goodsDescription.Add(formedPagesSupporter.Shipments[i].GoodsDescription);
				marksAndNumbers.Add(formedPagesSupporter.Shipments[i].MarksAndNumbers);
				packageCount.Add(formedPagesSupporter.Shipments[i].PackageCount);
				weight.Add(formedPagesSupporter.Shipments[i].Weight);
				volume.Add(formedPagesSupporter.Shipments[i].Volume);
			}

			FormatColumn[] columns = new FormatColumn[]
			{
				new FormatColumn(marksAndNumbers, shipmentWrapper.MarksAndNumbersWidth)
				{
					Heading = shipmentWrapper.MarksAndNumbersCaption,
					LeftPadding = shipmentWrapper.MarksAndNumbersLeftPadding,
				},

				new FormatColumn(packageCount, shipmentWrapper.PackagesWidth)
				{
					Heading = shipmentWrapper.PackagesCaption,
					LeftPadding = shipmentWrapper.PackagesLeftPadding,
					Options = FormatColumnOptions.RightAlign,
				},

				new FormatColumn(goodsDescription, shipmentWrapper.GoodsDescWidth)
				{
					Heading = shipmentWrapper.GoodsDescCaption,
					LeftPadding = shipmentWrapper.GoodsDescLeftPadding,
				},

				new FormatColumn(weight, shipmentWrapper.GrossWeightWidth)
				{
					Heading = shipmentWrapper.GrossWeightCaption,
					LeftPadding = shipmentWrapper.GrossWeightLeftPadding,
					Options = FormatColumnOptions.RightAlign,
				},

				new FormatColumn(volume, shipmentWrapper.VolumeMeasurementWidth)
				{
					Heading = shipmentWrapper.VolumeMeasurementCaption,
					LeftPadding = shipmentWrapper.VolumeMeasurementLeftPadding,
					Options = FormatColumnOptions.RightAlign,
				}
			};

			int[] indicies = new int[]
			{
				shipmentWrapper.MarksAndNumbersIndex,
				shipmentWrapper.PackagesIndex,
				shipmentWrapper.GoodsDescIndex,
				shipmentWrapper.GrossWeightIndex,
				shipmentWrapper.VolumeMeasurementIndex,
			};

			StripHiddenColumns(ref columns, ref indicies);
			Array.Sort(indicies, columns);
			return new FormatTable(shipmentWrapper.ShowDetailHeadingInMainBody, columns);
		}

		internal IFormatSectionComponent NewContainersTable()
		{
			if (shipmentWrapper.InterleavePacksAndContainers && !formedPagesSupporter.HidePackLinesInContainersSection)
			{
				return NewContainersTableWithInterleavedPacks();
			}
			else
			{
				return NewContainersTable(formedPagesSupporter.Containers, true);
			}
		}

		internal IFormatSectionComponent NewContainersTableWithInterleavedPacks()
		{
			List<FormatTable> tables = new List<FormatTable>();

			Dictionary<ZString, List<DocPackLines>> packsByContainer = new Dictionary<ZString, List<DocPackLines>>();

			if (formedPagesSupporter.DisplayContainers)
			{
				foreach (DocPackLines packWrapper in formedPagesSupporter.PackLines)
				{
					List<DocPackLines> lines;

					if (!packsByContainer.TryGetValue(packWrapper.ContainerCode, out lines))
					{
						lines = new List<DocPackLines>();
						packsByContainer.Add(packWrapper.ContainerCode, lines);
					}

					lines.Add(packWrapper);
				}

				foreach (DocFormedPagesContainer containerWrapper in formedPagesSupporter.Containers)
				{
					tables.Add(NewContainersTable(new object[] { containerWrapper }, false));

					List<DocPackLines> lines;

					if (packsByContainer.TryGetValue(containerWrapper.ContainerCode, out lines))
					{
						tables.Add(NewPacksTable(lines));
					}
				}

				{
					List<DocPackLines> lines;

					if (packsByContainer.TryGetValue(ZString.Empty, out lines))
					{
						tables.Add(NewContainersTable(Array.Empty<object>(), true));
						tables.Add(NewPacksTable(lines));
					}
				}
			}

			if (tables.Count == 1)
			{
				return tables[0];
			}
			else
			{
				return new FormatTableGroup(tables.ToArray());
			}
		}

		FormatTable NewContainersTable(IEnumerable containers, bool includeUnpacked)
		{
			List<string> containerNumber = new List<string>();
			List<string> containerSeal = new List<string>();
			List<string> containerType = new List<string>();
			List<string> containerWeight = new List<string>();
			List<string> containerWeightAndUQ = new List<string>();
			List<string> containerTare = new List<string>();
			List<string> containerTareAndUQ = new List<string>();
			List<string> containerGross = new List<string>();
			List<string> containerGrossAndUQ = new List<string>();
			List<string> containerVolume = new List<string>();
			List<string> containerVolumeAndUQ = new List<string>();
			List<string> containerPacks = new List<string>();
			List<string> containerDeliveryMode = new List<string>();
			List<string> containerTemperature = new List<string>();
			List<string> containerHumidity = new List<string>();

			if (formedPagesSupporter.DisplayContainers)
			{
				foreach (DocFormedPagesContainer container in containers)
				{
					containerNumber.Add(container.ContainerNumber);
					containerSeal.Add(container.ContainerSeal);
					containerType.Add(container.ContainerType);
					containerWeight.Add(FormatWeightNumber(ConvertWeight(container.ContainerNet, container.ContainerWeightUQ, true)));
					containerWeightAndUQ.Add(FormatWeightNumber(ConvertWeight(container.ContainerNet, container.ContainerWeightUQ)) + " " + GetTargetWeightUnit(container.ContainerWeightUQ));
					containerTare.Add(FormatWeightNumber(ConvertWeight(container.ContainerTare, container.ContainerWeightUQ, true)));
					containerTareAndUQ.Add(FormatWeightNumber(ConvertWeight(container.ContainerTare, container.ContainerWeightUQ)) + " " + GetTargetWeightUnit(container.ContainerWeightUQ));
					containerGross.Add(FormatWeightNumber(ConvertWeight(container.ContainerGross, container.ContainerWeightUQ, true)));
					containerGrossAndUQ.Add(FormatWeightNumber(ConvertWeight(container.ContainerGross, container.ContainerWeightUQ)) + " " + GetTargetWeightUnit(container.ContainerWeightUQ));
					containerVolume.Add(FormatVolumeNumber(ConvertVolume(container.Volume, container.VolumeUQ, true)));

					ZString formattedVolume = FormatVolumeNumber(ConvertVolume(container.Volume, container.VolumeUQ));
					containerVolumeAndUQ.Add(formattedVolume.IsEmpty ? "-" : formattedVolume + " " + GetTargetVolumeUnit(container.VolumeUQ));

					containerDeliveryMode.Add(container.DeliveryMode);
					containerTemperature.Add(container.Temperature);
					containerHumidity.Add(container.Humidity);

					string packsInfo = "";
					if (container.Packs > 0)
					{
						packsInfo = container.PackType.IsEmpty ? container.Packs.ToString() : string.Format("{0} {1}", container.Packs, container.PackType);
					}

					containerPacks.Add(packsInfo);
				}

				if (includeUnpacked)
				{
					foreach (DocPackLines pack in formedPagesSupporter.PackLines.Cast<DocPackLines>().Where(x => x.Container == null))
					{
						containerNumber.Add("-");
						containerSeal.Add("-");
						containerType.Add("-");
						containerWeight.Add(FormatWeightNumber(ConvertWeight(pack.ActualWeight, pack.ActualWeightUQ, true)));
						containerWeightAndUQ.Add(FormatWeightNumber(ConvertWeight(pack.ActualWeight, pack.ActualWeightUQ)) + " " + GetTargetWeightUnit(pack.ActualWeightUQ));
						containerTare.Add("-");
						containerTareAndUQ.Add("-");
						containerGross.Add(FormatWeightNumber(ConvertWeight(pack.ActualWeight, pack.ActualWeightUQ, true)));
						containerGrossAndUQ.Add(FormatWeightNumber(ConvertWeight(pack.ActualWeight, pack.ActualWeightUQ)) + " " + GetTargetWeightUnit(pack.ActualWeightUQ));
						containerVolume.Add(FormatVolumeNumber(ConvertVolume(pack.ActualVolume, pack.ActualVolumeUQ, true)));

						ZString formattedVolume = FormatVolumeNumber(ConvertVolume(pack.ActualVolume, pack.ActualVolumeUQ));
						containerVolumeAndUQ.Add(formattedVolume.IsEmpty ? "-" : formattedVolume + " " + pack.ActualVolumeUQ);

						containerDeliveryMode.Add("-");
						containerTemperature.Add("-");
						containerHumidity.Add("-");

						if (pack.PackType.IsEmpty)
						{
							containerPacks.Add(pack.PackageCount.ToString());
						}
						else
						{
							containerPacks.Add(string.Format("{0} {1}", pack.PackageCount, pack.PackType));
						}
					}
				}
			}

			FormatColumn[] columns = new FormatColumn[]
			{
				new FormatColumn(containerNumber, shipmentWrapper.ContainerNumberWidth)
				{
					Heading = shipmentWrapper.ContainerNumberCaption,
					LeftPadding = shipmentWrapper.ContainerNumberLeftPadding,
				},

				new FormatColumn(containerSeal, shipmentWrapper.ContainerSealWidth)
				{
					Heading = shipmentWrapper.ContainerSealCaption,
					LeftPadding = shipmentWrapper.ContainerSealLeftPadding,
				},

				new FormatColumn(containerType, shipmentWrapper.ContainerTypeWidth)
				{
					Heading = shipmentWrapper.ContainerTypeCaption,
					LeftPadding = shipmentWrapper.ContainerTypeLeftPadding,
				},

				new FormatColumn(containerWeight, shipmentWrapper.ContainerWeightWidth)
				{
					Heading = shipmentWrapper.ContainerWeightCaption,
					LeftPadding = shipmentWrapper.ContainerWeightLeftPadding,
					Options = FormatColumnOptions.RightAlign,
				},

				new FormatColumn(containerWeightAndUQ, shipmentWrapper.ContainerWeightAndUQWidth)
				{
					Heading = shipmentWrapper.ContainerWeightAndUQCaption,
					LeftPadding = shipmentWrapper.ContainerWeightAndUQLeftPadding,
					Options = FormatColumnOptions.RightAlign,
				},

				new FormatColumn(containerTare, shipmentWrapper.ContainerTareWidth)
				{
					Heading = shipmentWrapper.ContainerTareCaption,
					LeftPadding = shipmentWrapper.ContainerTareLeftPadding,
					Options = FormatColumnOptions.RightAlign,
				},

				new FormatColumn(containerTareAndUQ, shipmentWrapper.ContainerTareAndUQWidth)
				{
					Heading = shipmentWrapper.ContainerTareAndUQCaption,
					LeftPadding = shipmentWrapper.ContainerTareAndUQLeftPadding,
					Options = FormatColumnOptions.RightAlign,
				},

				new FormatColumn(containerGross, shipmentWrapper.ContainerGrossWidth)
				{
					Heading = shipmentWrapper.ContainerGrossCaption,
					LeftPadding = shipmentWrapper.ContainerGrossLeftPadding,
					Options = FormatColumnOptions.RightAlign,
				},

				new FormatColumn(containerGrossAndUQ, shipmentWrapper.ContainerGrossAndUQWidth)
				{
					Heading = shipmentWrapper.ContainerGrossAndUQCaption,
					LeftPadding = shipmentWrapper.ContainerGrossAndUQLeftPadding,
					Options = FormatColumnOptions.RightAlign,
				},

				new FormatColumn(containerVolume, shipmentWrapper.ContainerVolumeWidth)
				{
					Heading = shipmentWrapper.ContainerVolumeCaption,
					LeftPadding = shipmentWrapper.ContainerVolumeLeftPadding,
					Options = FormatColumnOptions.RightAlign,
				},

				new FormatColumn(containerVolumeAndUQ, shipmentWrapper.ContainerVolumeAndUQWidth)
				{
					Heading = shipmentWrapper.ContainerVolumeAndUQCaption,
					LeftPadding = shipmentWrapper.ContainerVolumeAndUQLeftPadding,
					Options = FormatColumnOptions.RightAlign,
				},

				new FormatColumn(containerPacks, shipmentWrapper.ContainerPackagesWidth)
				{
					Heading = shipmentWrapper.ContainerPackagesCaption,
					LeftPadding = shipmentWrapper.ContainerPackagesLeftPadding,
					Options = FormatColumnOptions.RightAlign,
				},

				new FormatColumn(containerDeliveryMode, shipmentWrapper.ContainerModeWidth)
				{
					Heading = shipmentWrapper.ContainerModeCaption,
					LeftPadding = shipmentWrapper.ContainerModeLeftPadding,
					Options = FormatColumnOptions.RightAlign,
				},

				new FormatColumn(containerTemperature, shipmentWrapper.ContainerTemperatureSettingWidth)
				{
					Heading = shipmentWrapper.ContainerTemperatureSettingCaption,
					LeftPadding = shipmentWrapper.ContainerTemperatureSettingLeftPadding,
					Options = FormatColumnOptions.RightAlign,
				},

				new FormatColumn(containerHumidity, shipmentWrapper.ContainerHumiditySettingWidth)
				{
					Heading = shipmentWrapper.ContainerHumiditySettingCaption,
					LeftPadding = shipmentWrapper.ContainerHumiditySettingLeftPadding,
					Options = FormatColumnOptions.RightAlign,
				},
			};

			int[] indicies = new int[]
			{
				shipmentWrapper.ContainerNumberIndex,
				shipmentWrapper.ContainerSealIndex,
				shipmentWrapper.ContainerTypeIndex,
				shipmentWrapper.ContainerWeightIndex,
				shipmentWrapper.ContainerWeightAndUQIndex,
				formedPagesSupporter.HideContainerTareWeight ? (ZInt)0 : shipmentWrapper.ContainerTareIndex,
				formedPagesSupporter.HideContainerTareWeight ? (ZInt)0 : shipmentWrapper.ContainerTareAndUQIndex,
				formedPagesSupporter.HideContainerGrossWeight ? (ZInt)0 : shipmentWrapper.ContainerGrossIndex,
				formedPagesSupporter.HideContainerGrossWeight ? (ZInt)0 : shipmentWrapper.ContainerGrossAndUQIndex,
				shipmentWrapper.ContainerVolumeIndex,
				shipmentWrapper.ContainerVolumeAndUQIndex,
				shipmentWrapper.ContainerPackagesIndex,
				shipmentWrapper.ContainerModeIndex,
				shipmentWrapper.ContainerTemperatureSettingIndex,
				shipmentWrapper.ContainerHumiditySettingIndex,
			};

			StripHiddenColumns(ref columns, ref indicies);
			Array.Sort(indicies, columns);
			return new FormatTable(shipmentWrapper.ShowContainerHeadingInMainBody, columns);
		}

		internal IFormatSectionComponent NewTopLevelPacksTable()
		{
			return NewPacksTable(!shipmentWrapper.PackingMode.IsEmpty
				? formedPagesSupporter.TopLevelPacks.FilteredByMode(shipmentWrapper.PackingMode).Cast<IDocPackageDetails>()
				: Enumerable.Empty<IDocPackageDetails>());
		}

		internal FormatTable NewPacksTable(IEnumerable<IDocPackageDetails> packages)
		{
			List<string> packRefNumber = new List<string>();
			List<string> packDescription = new List<string>();
			List<string> packShortDescription = new List<string>();
			List<string> packMarksAndNumbers = new List<string>();
			List<string> packContainsUNDG = new List<string>();
			List<string> packUNDG = new List<string>();
			List<string> packDescriptionAndUNDGs = new List<string>();
			List<string> packUNDGsAndShortDescription = new List<string>();
			List<string> packRefNumberAndMarksAndNumbers = new List<string>();
			List<string> packCommodityCode = new List<string>();
			List<string> packCommodityDescription = new List<string>();
			List<string> packCount = new List<string>();
			List<string> packCountAndType = new List<string>();
			List<string> packWeight = new List<string>();
			List<string> packWeightAndUQ = new List<string>();
			List<string> packVolume = new List<string>();
			List<string> packVolumeAndUQ = new List<string>();
			List<string> packLength = new List<string>();
			List<string> packLengthAndUQ = new List<string>();
			List<string> packWidth = new List<string>();
			List<string> packWidthAndUQ = new List<string>();
			List<string> packHeight = new List<string>();
			List<string> packHeightAndUQ = new List<string>();
			List<string> packArea = new List<string>();
			List<string> packDimensions = new List<string>();
			List<string> packHarmonizedCode = new List<string>();
			List<string> vehicleColour = new List<string>();
			List<string> vehicleMake = new List<string>();
			List<string> vehicleModel = new List<string>();
			List<string> vehicleNumberOfDoors = new List<string>();
			List<string> vehicleTransmission = new List<string>();
			List<string> vehicleYear = new List<string>();
			List<string> vehicleFullDetails = new List<string>();

			foreach (IDocPackageDetails packageDetails in packages)
			{
				packRefNumber.Add(packageDetails.ReferenceNumber);
				packDescription.Add(packageDetails.DetailedDescription);
				packShortDescription.Add(packageDetails.Description);
				packMarksAndNumbers.Add(packageDetails.MarksAndNumbers);

				var builder = new ZStringBuilder();
				builder.AppendIfNotEmpty(packageDetails.ReferenceNumber);
				builder.AppendIfNotEmpty(packageDetails.MarksAndNumbers);
				packRefNumberAndMarksAndNumbers.Add(builder.ToStringWithNewLineBetweenAppends());

				ZString hazardousDescription = HazardousDescription(packageDetails.UNDGs, shipmentWrapper.IncludeEmergencyContactWithUNDG);
				packUNDG.Add(hazardousDescription);

				var descriptionAndUNDGs = new ZStringBuilder();
				descriptionAndUNDGs.AppendIfNotEmpty(packageDetails.DetailedDescription);
				descriptionAndUNDGs.AppendIfNotEmpty(hazardousDescription);
				packDescriptionAndUNDGs.Add(descriptionAndUNDGs.ToStringWithNewLineBetweenAppends());

				var undgsAndShortDescription = new ZStringBuilder();
				undgsAndShortDescription.AppendIfNotEmpty(hazardousDescription);
				undgsAndShortDescription.AppendIfNotEmpty(packageDetails.Description);
				packUNDGsAndShortDescription.Add(undgsAndShortDescription.ToStringWithNewLineBetweenAppends());

				packContainsUNDG.Add(packageDetails.UNDGs.Length > 0 ? "X" : "");

				packCommodityCode.Add(packageDetails.Commodity != null ? packageDetails.Commodity.Code : ZString.Empty);
				packCommodityDescription.Add(packageDetails.Commodity != null ? packageDetails.Commodity.Description : ZString.Empty);

				packCount.Add(packageDetails.Count.ToString());
				packCountAndType.Add(packageDetails.Count.ToString() + " " + packageDetails.PackType);

				packWeight.Add(FormatWeightNumber(ConvertWeight(packageDetails.Weight, packageDetails.WeightUnit, true)));
				packWeightAndUQ.Add(FormatWeightNumber(ConvertWeight(packageDetails.Weight, packageDetails.WeightUnit)) + " " + GetTargetWeightUnit(packageDetails.WeightUnit));

				packVolume.Add(FormatVolumeNumber(ConvertVolume(packageDetails.Volume, packageDetails.VolumeUnit, true)));

				ZString formattedVolume = FormatVolumeNumber(ConvertVolume(packageDetails.Volume, packageDetails.VolumeUnit));
				packVolumeAndUQ.Add(formattedVolume.IsEmpty ? "-" : formattedVolume + " " + GetTargetVolumeUnit(packageDetails.VolumeUnit));

				packLength.Add(shipmentWrapper.FormatNumber(ConvertLength(packageDetails.Length, packageDetails.DimensionUnit, true), shipmentWrapper.DimensionsDecimalPlaces));
				packLengthAndUQ.Add(shipmentWrapper.FormatNumber(ConvertLength(packageDetails.Length, packageDetails.DimensionUnit), shipmentWrapper.DimensionsDecimalPlaces)
					+ " " + GetTargetLengthUnit(packageDetails.DimensionUnit));

				packWidth.Add(shipmentWrapper.FormatNumber(ConvertLength(packageDetails.Width, packageDetails.DimensionUnit, true), shipmentWrapper.DimensionsDecimalPlaces));
				packWidthAndUQ.Add(shipmentWrapper.FormatNumber(ConvertLength(packageDetails.Width, packageDetails.DimensionUnit), shipmentWrapper.DimensionsDecimalPlaces)
					+ " " + GetTargetLengthUnit(packageDetails.DimensionUnit));

				packHeight.Add(shipmentWrapper.FormatNumber(ConvertLength(packageDetails.Height, packageDetails.DimensionUnit, true), shipmentWrapper.DimensionsDecimalPlaces));
				packHeightAndUQ.Add(shipmentWrapper.FormatNumber(ConvertLength(packageDetails.Height, packageDetails.DimensionUnit), shipmentWrapper.DimensionsDecimalPlaces)
					+ " " + GetTargetLengthUnit(packageDetails.DimensionUnit));

				packArea.Add(shipmentWrapper.FormatNumber(ConvertLength(packageDetails.Length, packageDetails.DimensionUnit, true)
					* ConvertLength(packageDetails.Width, packageDetails.DimensionUnit, true) * packageDetails.Count, shipmentWrapper.AreaDecimalPlaces));

				packDimensions.Add(FormatDimensions(ConvertVolume(packageDetails.Volume, packageDetails.VolumeUnit), GetTargetVolumeUnit(packageDetails.VolumeUnit),
					ConvertLength(packageDetails.Length, packageDetails.DimensionUnit), ConvertLength(packageDetails.Width, packageDetails.DimensionUnit),
					ConvertLength(packageDetails.Height, packageDetails.DimensionUnit), GetTargetLengthUnit(packageDetails.DimensionUnit)));

				packHarmonizedCode.Add(packageDetails.HarmonisedCode);

				vehicleColour.Add(packageDetails.VehicleColour);
				vehicleMake.Add(packageDetails.VehicleMake);
				vehicleModel.Add(packageDetails.VehicleModel);
				vehicleNumberOfDoors.Add(packageDetails.VehicleNumberOfDoors.ToString());
				vehicleTransmission.Add(packageDetails.VehicleTransmission);
				vehicleYear.Add(packageDetails.VehicleYear.ToString());

				vehicleFullDetails.Add(FormatVehicleFullDetails(packageDetails.VehicleColour, packageDetails.VehicleMake, packageDetails.VehicleModel, packageDetails.VehicleNumberOfDoors,
						packageDetails.VehicleTransmission, packageDetails.VehicleYear, packageDetails.DetailedDescription, HazardousDescription(packageDetails.UNDGs, shipmentWrapper.IncludeEmergencyContactWithUNDG)));
			}

			FormatColumn[] columns = new FormatColumn[]
			{
				new FormatColumn(packRefNumber, shipmentWrapper.PackRefNumberColumnWidth)
				{
					Heading = shipmentWrapper.PackRefNumberColumnCaption,
					LeftPadding = shipmentWrapper.PackRefNumberColumnLeftPadding,
				},

				new FormatColumn(packDescription, shipmentWrapper.PackDescriptionColumnWidth)
				{
					Heading = shipmentWrapper.PackDescriptionColumnCaption,
					LeftPadding = shipmentWrapper.PackDescriptionColumnLeftPadding,
				},

				new FormatColumn(packShortDescription, shipmentWrapper.PackShortDescriptionColumnWidth)
				{
					Heading = shipmentWrapper.PackShortDescriptionColumnCaption,
					LeftPadding = shipmentWrapper.PackShortDescriptionColumnLeftPadding,
				},

				new FormatColumn(packMarksAndNumbers, shipmentWrapper.PackMarksAndNumbersColumnWidth)
				{
					Heading = shipmentWrapper.PackMarksAndNumbersColumnCaption,
					LeftPadding = shipmentWrapper.PackMarksAndNumbersColumnLeftPadding,
				},

				new FormatColumn(packContainsUNDG, shipmentWrapper.PackContainsUNDGColumnWidth)
				{
					Heading = shipmentWrapper.PackContainsUNDGColumnCaption,
					LeftPadding = shipmentWrapper.PackContainsUNDGColumnLeftPadding,
				},

				new FormatColumn(packUNDG, shipmentWrapper.PackUNDGColumnWidth)
				{
					Heading = shipmentWrapper.PackUNDGColumnCaption,
					LeftPadding = shipmentWrapper.PackUNDGColumnLeftPadding,
				},

				new FormatColumn(packDescriptionAndUNDGs, shipmentWrapper.PackDescriptionAndUNDGColumnWidth)
				{
					Heading = shipmentWrapper.PackDescriptionAndUNDGColumnCaption,
					LeftPadding = shipmentWrapper.PackDescriptionAndUNDGColumnLeftPadding,
				},

				new FormatColumn(packUNDGsAndShortDescription, shipmentWrapper.PackUNDGAndShortDescriptionColumnWidth)
				{
					Heading = shipmentWrapper.PackUNDGAndShortDescriptionColumnCaption,
					LeftPadding = shipmentWrapper.PackUNDGAndShortDescriptionColumnLeftPadding,
				},

				new FormatColumn(packRefNumberAndMarksAndNumbers, shipmentWrapper.PackRefNumberAndMarksAndNumbersColumnWidth)
				{
					Heading = shipmentWrapper.PackRefNumberAndMarksAndNumbersColumnCaption,
					LeftPadding = shipmentWrapper.PackRefNumberAndMarksAndNumbersColumnLeftPadding,
				},

				new FormatColumn(packCommodityCode, shipmentWrapper.PackCommodityCodeColumnWidth)
				{
					Heading = shipmentWrapper.PackCommodityCodeColumnCaption,
					LeftPadding = shipmentWrapper.PackCommodityCodeColumnLeftPadding,
				},

				new FormatColumn(packCommodityDescription, shipmentWrapper.PackCommodityDescriptionColumnWidth)
				{
					Heading = shipmentWrapper.PackCommodityDescriptionColumnCaption,
					LeftPadding = shipmentWrapper.PackCommodityDescriptionColumnLeftPadding,
				},

				new FormatColumn(packCount, shipmentWrapper.PackCountColumnWidth)
				{
					Heading = shipmentWrapper.PackCountColumnCaption,
					LeftPadding = shipmentWrapper.PackCountColumnLeftPadding,
					Options = FormatColumnOptions.RightAlign,
				},

				new FormatColumn(packCountAndType, shipmentWrapper.PackCountAndTypeColumnWidth)
				{
					Heading = shipmentWrapper.PackCountAndTypeColumnCaption,
					LeftPadding = shipmentWrapper.PackCountAndTypeColumnLeftPadding,
					Options = FormatColumnOptions.RightAlign,
				},

				new FormatColumn(packWeight, shipmentWrapper.PackWeightColumnWidth)
				{
					Heading = shipmentWrapper.PackWeightColumnCaption,
					LeftPadding = shipmentWrapper.PackWeightColumnLeftPadding,
					Options = FormatColumnOptions.RightAlign,
				},

				new FormatColumn(packWeightAndUQ, shipmentWrapper.PackWeightAndUQColumnWidth)
				{
					Heading = shipmentWrapper.PackWeightAndUQColumnCaption,
					LeftPadding = shipmentWrapper.PackWeightAndUQColumnLeftPadding,
					Options = FormatColumnOptions.RightAlign,
				},

				new FormatColumn(packVolume, shipmentWrapper.PackVolumeColumnWidth)
				{
					Heading = shipmentWrapper.PackVolumeColumnCaption,
					LeftPadding = shipmentWrapper.PackVolumeColumnLeftPadding,
					Options = FormatColumnOptions.RightAlign,
				},

				new FormatColumn(packVolumeAndUQ, shipmentWrapper.PackVolumeAndUQColumnWidth)
				{
					Heading = shipmentWrapper.PackVolumeAndUQColumnCaption,
					LeftPadding = shipmentWrapper.PackVolumeAndUQColumnLeftPadding,
					Options = FormatColumnOptions.RightAlign,
				},

				new FormatColumn(packLength, shipmentWrapper.PackLengthColumnWidth)
				{
					Heading = shipmentWrapper.PackLengthColumnCaption,
					LeftPadding = shipmentWrapper.PackLengthColumnLeftPadding,
					Options = FormatColumnOptions.RightAlign,
				},

				new FormatColumn(packLengthAndUQ, shipmentWrapper.PackLengthAndUQColumnWidth)
				{
					Heading = shipmentWrapper.PackLengthAndUQColumnCaption,
					LeftPadding = shipmentWrapper.PackLengthAndUQColumnLeftPadding,
					Options = FormatColumnOptions.RightAlign,
				},

				new FormatColumn(packWidth, shipmentWrapper.PackWidthColumnWidth)
				{
					Heading = shipmentWrapper.PackWidthColumnCaption,
					LeftPadding = shipmentWrapper.PackWidthColumnLeftPadding,
					Options = FormatColumnOptions.RightAlign,
				},

				new FormatColumn(packWidthAndUQ, shipmentWrapper.PackWidthAndUQColumnWidth)
				{
					Heading = shipmentWrapper.PackWidthAndUQColumnCaption,
					LeftPadding = shipmentWrapper.PackWidthAndUQColumnLeftPadding,
					Options = FormatColumnOptions.RightAlign,
				},

				new FormatColumn(packHeight, shipmentWrapper.PackHeightColumnWidth)
				{
					Heading = shipmentWrapper.PackHeightColumnCaption,
					LeftPadding = shipmentWrapper.PackHeightColumnLeftPadding,
					Options = FormatColumnOptions.RightAlign,
				},

				new FormatColumn(packHeightAndUQ, shipmentWrapper.PackHeightAndUQColumnWidth)
				{
					Heading = shipmentWrapper.PackHeightAndUQColumnCaption,
					LeftPadding = shipmentWrapper.PackHeightAndUQColumnLeftPadding,
					Options = FormatColumnOptions.RightAlign,
				},

				new FormatColumn(packArea, shipmentWrapper.PackAreaColumnWidth)
				{
					Heading = shipmentWrapper.PackAreaColumnCaption,
					LeftPadding = shipmentWrapper.PackAreaColumnLeftPadding,
					Options = FormatColumnOptions.RightAlign,
				},

				new FormatColumn(packDimensions, shipmentWrapper.PackDimensionsColumnWidth)
				{
					Heading = shipmentWrapper.PackDimensionsColumnCaption,
					LeftPadding = shipmentWrapper.PackDimensionsColumnLeftPadding,
					Options = FormatColumnOptions.RightAlign,
				},

				new FormatColumn(packHarmonizedCode, shipmentWrapper.PackHarmonizedCodeColumnWidth)
				{
					Heading = shipmentWrapper.PackHarmonizedCodeColumnCaption,
					LeftPadding = shipmentWrapper.PackHarmonizedCodeColumnLeftPadding,
				},

				new FormatColumn(vehicleColour, shipmentWrapper.PackVehicleColourColumnWidth)
				{
					Heading = shipmentWrapper.PackVehicleColourColumnCaption,
					LeftPadding = shipmentWrapper.PackVehicleColourColumnLeftPadding,
				},

				new FormatColumn(vehicleMake, shipmentWrapper.PackVehicleMakeColumnWidth)
				{
					Heading = shipmentWrapper.PackVehicleMakeColumnCaption,
					LeftPadding = shipmentWrapper.PackVehicleMakeColumnLeftPadding,
				},

				new FormatColumn(vehicleModel, shipmentWrapper.PackVehicleModelColumnWidth)
				{
					Heading = shipmentWrapper.PackVehicleModelColumnCaption,
					LeftPadding = shipmentWrapper.PackVehicleModelColumnLeftPadding,
				},

				new FormatColumn(vehicleNumberOfDoors, shipmentWrapper.PackVehicleNumberOfDoorsColumnWidth)
				{
					Heading = shipmentWrapper.PackVehicleNumberOfDoorsColumnCaption,
					LeftPadding = shipmentWrapper.PackVehicleNumberOfDoorsColumnLeftPadding,
					Options = FormatColumnOptions.RightAlign,
				},

				new FormatColumn(vehicleTransmission, shipmentWrapper.PackVehicleTransmissionColumnWidth)
				{
					Heading = shipmentWrapper.PackVehicleTransmissionColumnCaption,
					LeftPadding = shipmentWrapper.PackVehicleTransmissionColumnLeftPadding,
				},

				new FormatColumn(vehicleYear, shipmentWrapper.PackVehicleYearColumnWidth)
				{
					Heading = shipmentWrapper.PackVehicleYearColumnCaption,
					LeftPadding = shipmentWrapper.PackVehicleYearColumnLeftPadding,
					Options = FormatColumnOptions.RightAlign,
				},

				new FormatColumn(vehicleFullDetails, shipmentWrapper.PackVehicleFullDetailsColumnWidth)
				{
					Heading = shipmentWrapper.PackVehicleFullDetailsColumnCaption,
					LeftPadding = shipmentWrapper.PackVehicleFullDetailsColumnLeftPadding,
				},
			};

			int[] indicies = new int[]
			{
				shipmentWrapper.PackRefNumberColumnIndex,
				shipmentWrapper.PackDescriptionColumnIndex,
				shipmentWrapper.PackShortDescriptionColumnIndex,
				shipmentWrapper.PackMarksAndNumbersColumnIndex,
				shipmentWrapper.PackContainsUNDGColumnIndex,
				shipmentWrapper.PackUNDGColumnIndex,
				shipmentWrapper.PackDescriptionAndUNDGColumnIndex,
				shipmentWrapper.PackUNDGAndShortDescriptionColumnIndex,
				shipmentWrapper.PackRefNumberAndMarksAndNumbersColumnIndex,
				shipmentWrapper.PackCommodityCodeColumnIndex,
				shipmentWrapper.PackCommodityDescriptionColumnIndex,
				shipmentWrapper.PackCountColumnIndex,
				shipmentWrapper.PackCountAndTypeColumnIndex,
				shipmentWrapper.PackWeightColumnIndex,
				shipmentWrapper.PackWeightAndUQColumnIndex,
				shipmentWrapper.PackVolumeColumnIndex,
				shipmentWrapper.PackVolumeAndUQColumnIndex,
				shipmentWrapper.PackLengthColumnIndex,
				shipmentWrapper.PackLengthAndUQColumnIndex,
				shipmentWrapper.PackWidthColumnIndex,
				shipmentWrapper.PackWidthAndUQColumnIndex,
				shipmentWrapper.PackHeightColumnIndex,
				shipmentWrapper.PackHeightAndUQColumnIndex,
				shipmentWrapper.PackAreaColumnIndex,
				shipmentWrapper.PackDimensionsColumnIndex,
				shipmentWrapper.PackHarmonizedCodeColumnIndex,
				shipmentWrapper.PackVehicleColourColumnIndex,
				shipmentWrapper.PackVehicleMakeColumnIndex,
				shipmentWrapper.PackVehicleModelColumnIndex,
				shipmentWrapper.PackVehicleNumberOfDoorsColumnIndex,
				shipmentWrapper.PackVehicleTransmissionColumnIndex,
				shipmentWrapper.PackVehicleYearColumnIndex,
				shipmentWrapper.PackVehicleFullDetailsColumnIndex,
			};

			StripHiddenColumns(ref columns, ref indicies);
			Array.Sort(indicies, columns);
			return new FormatTable(shipmentWrapper.ShowRORHeadingInMainBody, columns);
		}

		static class ChargesDisplayTypes
		{
			public const string NoCharges = "NON";
			public const string CollectCharges = "SHW";
			public const string PrepaidCharges = "PPD";
			public const string AsAgreed = "AGR";
			public const string AllCharges = "ALL";
			public const string OriginalAsAgreedCopyWithCollectCharges = "CCL";
			public const string OriginalAsAgreedCopyWithPrepaidCharges = "CPP";
			public const string OriginalAsAgreedCopyWithPrepaidAndCollectCharges = "CAL";
		}

		internal IFormatSectionComponent NewChargesTable()
		{
			switch (shipmentWrapper.ChargesDisplay)
			{
				case ChargesDisplayTypes.CollectCharges:
					return NewChargesTable(false, true);

				case ChargesDisplayTypes.PrepaidCharges:
					return NewChargesTable(true, false);

				case ChargesDisplayTypes.AsAgreed:
					return NewTextTable(9, Res.GetString("ea99d432-327f-44ee-aa19-1f63fa36cc10", "As Agreed"));

				case ChargesDisplayTypes.AllCharges:
					return NewChargesTable(true, true);

				case ChargesDisplayTypes.OriginalAsAgreedCopyWithCollectCharges:
					if (formedPagesSupporter.IsOriginal)
					{
						return NewTextTable(9, Res.GetString("ea99d432-327f-44ee-aa19-1f63fa36cc10", "As Agreed"));
					}
					else if (formedPagesSupporter.IsCopy)
					{
						return NewChargesTable(false, true);
					}

					return NewTextTable(0, "");

				case ChargesDisplayTypes.OriginalAsAgreedCopyWithPrepaidCharges:
					if (formedPagesSupporter.IsOriginal)
					{
						return NewTextTable(9, Res.GetString("ea99d432-327f-44ee-aa19-1f63fa36cc10", "As Agreed"));
					}
					else if (formedPagesSupporter.IsCopy)
					{
						return NewChargesTable(true, false);
					}

					return NewTextTable(0, "");

				case ChargesDisplayTypes.OriginalAsAgreedCopyWithPrepaidAndCollectCharges:
					if (formedPagesSupporter.IsOriginal)
					{
						return NewTextTable(9, Res.GetString("ea99d432-327f-44ee-aa19-1f63fa36cc10", "As Agreed"));
					}
					else if (formedPagesSupporter.IsCopy)
					{
						return NewChargesTable(true, true);
					}

					return NewTextTable(0, "");

				case ChargesDisplayTypes.NoCharges:
				default:
					return NewTextTable(0, "");
			}
		}

		internal IFormatSectionComponent NewChargesTable(bool showPrepaid, bool showCollect)
		{
			return formedPagesSupporter.ShouldPrintChargesAsLumpSum ? NewChargesTableAsLumpSum(showPrepaid, showCollect) : NewChargesTableAsRateLines(showPrepaid, showCollect);
		}

		IFormatSectionComponent NewChargesTableAsLumpSum(bool showPrepaid, bool showCollect)
		{
			ZInt combinedColumnWidth = 0;

			if (shipmentWrapper.ChargeCodeIndex > 0 && shipmentWrapper.ChargeCodeColumnWidth > 0)
			{
				combinedColumnWidth += shipmentWrapper.ChargeCodeLeftPadding + shipmentWrapper.ChargeCodeColumnWidth;
			}

			if (shipmentWrapper.ChargeCodeDescIndex > 0 && shipmentWrapper.ChargeCodeDescColumnWidth > 0)
			{
				combinedColumnWidth += shipmentWrapper.ChargeCodeDescLeftPadding + shipmentWrapper.ChargeCodeDescColumnWidth;
			}

			if (shipmentWrapper.ChargeDescriptionIndex > 0 && shipmentWrapper.ChargeDescriptionColumnWidth > 0)
			{
				combinedColumnWidth += shipmentWrapper.ChargeDescriptionLeftPadding + shipmentWrapper.ChargeDescriptionColumnWidth;
			}

			if (shipmentWrapper.CollectChargesColumnIndex > 0 && shipmentWrapper.CollectChargesColumnWidth > 0)
			{
				combinedColumnWidth += shipmentWrapper.CollectChargesColumnLeftPadding + shipmentWrapper.CollectChargesColumnWidth;
			}

			if (shipmentWrapper.CollectCurrencyColumnIndex > 0 && shipmentWrapper.CollectCurrencyColumnWidth > 0)
			{
				combinedColumnWidth += shipmentWrapper.CollectCurrencyColumnLeftPadding + shipmentWrapper.CollectCurrencyColumnWidth;
			}

			if (shipmentWrapper.PrepaidChargesColumnIndex > 0 && shipmentWrapper.PrepaidChargesColumnWidth > 0)
			{
				combinedColumnWidth += shipmentWrapper.PrepaidChargesColumnLeftPadding + shipmentWrapper.PrepaidChargesColumnWidth;
			}

			if (shipmentWrapper.PrepaidCurrencyColumnIndex > 0 && shipmentWrapper.PrepaidCurrencyColumnWidth > 0)
			{
				combinedColumnWidth += shipmentWrapper.PrepaidCurrencyColumnLeftPadding + shipmentWrapper.PrepaidCurrencyColumnWidth;
			}

			if (shipmentWrapper.AllChargesColumnIndex > 0 && shipmentWrapper.AllChargesColumnWidth > 0)
			{
				combinedColumnWidth += shipmentWrapper.AllChargesColumnLeftPadding + shipmentWrapper.AllChargesColumnWidth;
			}

			if (shipmentWrapper.AllChargesCurrencyColumnIndex > 0 && shipmentWrapper.AllChargesCurrencyColumnWidth > 0)
			{
				combinedColumnWidth += shipmentWrapper.AllChargesCurrencyColumnLeftPadding + shipmentWrapper.AllChargesCurrencyColumnWidth;
			}

			return NewTextTable(combinedColumnWidth, GetChargesAsLumpSum(showPrepaid, showCollect));
		}

		ZString GetChargesAsLumpSum(bool showPrepaid, bool showCollect)
		{
			DocCurrency combinedChargesCurrency = null;
			bool multipleCurrencies = false;
			ZDecimal combinedCharges = 0;
			ZString combinedChargesString = ZString.Empty;

			var allCharges = formedPagesSupporter.AllCharges.Cast<DocJobCharge>().Where(charge => IncludeChargeInCalculation(charge, showPrepaid, showCollect)).ToList();

			foreach (DocJobCharge charge in allCharges)
			{
				if (combinedChargesCurrency == null)
				{
					combinedChargesCurrency = charge.OSSellCurrency;
				}
				else if (combinedChargesCurrency.Code != charge.OSSellCurrency.Code)
				{
					multipleCurrencies = true;
					break;
				}

				combinedCharges += charge.OSSellAmt;
			}

			if (multipleCurrencies)
			{
				combinedCharges = 0;

				var converter = GetCurrencyConverter();
				var usd = Factory.LoadFromNaturalKey<RefCurrency>(RefCurrencySchema.RX_Code, Constants.CurrencyCodes.UnitedStates);
				combinedChargesCurrency = DocCurrency.New(Factory, usd);

				foreach (DocJobCharge charge in allCharges)
				{
					var money = new Money(charge.OSSellAmt, charge.OSSellCurrency);
					combinedCharges += converter.ConvertRounded(money, usd).Amount;
				}
			}

			if (combinedCharges > 0 && combinedChargesCurrency != null)
			{
				var converter = new CurrencyToWords_EN();
				ZString inWords = converter.ConvertToWords(Convert.ToDouble(combinedCharges), combinedChargesCurrency.Code).Replace(" only", "");
				combinedChargesString = (NoResString)"FREIGHT LUMP SUM: " + new Money(combinedCharges, combinedChargesCurrency).ToString() + (NoResString)"\r\n" + inWords.ToUpper();
			}

			return combinedChargesString;
		}

		CurrencyConverter GetCurrencyConverter()
		{
			return shipmentWrapper?.JobHeader?.JobHeader?.CurrencyConverter ?? CurrencyConverter.New(Factory, ZDateTime.Now, ExchangeRateType.Sell, 1);
		}

		bool IncludeChargeInCalculation(DocJobCharge charge, bool showPrepaid, bool showCollect)
		{
			return (charge.OSSellAmt > 0 || !shipmentWrapper.SuppressZeroCharges)
				&& IsNotProfitShareCharge(charge)
				&& ((showCollect && showPrepaid) || (showCollect && IsCollect(charge)) || (showPrepaid && IsPrepaid(charge)));
		}

		bool IsNotProfitShareCharge(DocJobCharge charge) => charge.ChargeCode.AccChargeCode.PK != AccountingConfigurationRegistry.Instance.ProfitShareChargeCode.Value;

		IFormatSectionComponent NewChargesTableAsRateLines(bool showPrepaid, bool showCollect)
		{
			List<string> codes = new List<string>();
			List<string> descriptions = new List<string>();
			List<string> collectAmt = new List<string>();
			List<string> collectCurrency = new List<string>();
			List<string> prepaidAmt = new List<string>();
			List<string> prepaidCurrency = new List<string>();
			List<string> allAmt = new List<string>();
			List<string> allCurrency = new List<string>();
			List<string> chargeCodeDesc = new List<string>();

			var sumPrepaidCharges = 0m;
			var sumCollectCharges = 0m;

			var allPrepaidCollectCharges = formedPagesSupporter.AllCharges.Cast<DocJobCharge>().Where(charge => IncludeChargeInCalculation(charge, showPrepaid, showCollect)).ToList();

			if (allPrepaidCollectCharges.Count > 0)
			{
				var convertChargeAmountToUSD = formedPagesSupporter.ShouldPrintTotalCharges && allPrepaidCollectCharges.Any(x => x.OSSellCurrency != allPrepaidCollectCharges[0].OSSellCurrency);
				var usd = Factory.LoadFromNaturalKey<RefCurrency>(RefCurrencySchema.RX_Code, Constants.CurrencyCodes.UnitedStates);

				foreach (DocJobCharge charge in allPrepaidCollectCharges)
				{
					string chargeOSSellAmountText;
					string chargeOsSellCurrency;
					var chargeOSSellAmount = 0m;

					if (convertChargeAmountToUSD)
					{
						var convertedChargeMoney = GetChargeAmountWithCurrency(charge, usd);
						chargeOSSellAmountText = GetAmountTextByCurrency(convertedChargeMoney.Amount, usd);
						chargeOsSellCurrency = convertedChargeMoney.Currency.Code;
						chargeOSSellAmount = convertedChargeMoney.Amount;
					}
					else
					{
						chargeOSSellAmountText = GetAmountTextByCurrency(charge.OSSellAmt, charge.OSSellCurrency);
						chargeOsSellCurrency = charge.OSSellCurrency.Code;
						chargeOSSellAmount = charge.OSSellAmt;
					}

					if (IsCollect(charge))
					{
						if (showCollect)
						{
							codes.Add(charge.ChargeCode.Code);
							chargeCodeDesc.Add(charge.ChargeCode.Desc);
							descriptions.Add(charge.Description);
							collectAmt.Add(chargeOSSellAmountText);
							collectCurrency.Add(chargeOsSellCurrency);
							prepaidAmt.Add("");
							prepaidCurrency.Add("");
							allAmt.Add(chargeOSSellAmountText);
							allCurrency.Add(chargeOsSellCurrency);

							sumCollectCharges += chargeOSSellAmount;
						}
					}
					else if (showPrepaid)
					{
						codes.Add(charge.ChargeCode.Code);
						chargeCodeDesc.Add(charge.ChargeCode.Desc);
						descriptions.Add(charge.Description);
						collectAmt.Add("");
						collectCurrency.Add("");
						prepaidAmt.Add(chargeOSSellAmountText);
						prepaidCurrency.Add(chargeOsSellCurrency);
						allAmt.Add(chargeOSSellAmountText);
						allCurrency.Add(chargeOsSellCurrency);

						sumPrepaidCharges += chargeOSSellAmount;
					}
				}

				if (formedPagesSupporter.ShouldPrintTotalCharges)
				{
					var sumChargesAmount = sumPrepaidCharges + sumCollectCharges;
					var totalChargeAmountCurrencyCode = convertChargeAmountToUSD ? Constants.CurrencyCodes.UnitedStates : allPrepaidCollectCharges[0].OSSellCurrency.Code.ToString();
					var chargeCurrency = Factory.LoadFromNaturalKey<RefCurrency>(RefCurrencySchema.RX_Code, totalChargeAmountCurrencyCode);

					if (sumChargesAmount > 0)
					{
						var totalPrepaidCharges = GetAmountTextByCurrency(sumPrepaidCharges, chargeCurrency);
						var totalCollectCharges = GetAmountTextByCurrency(sumCollectCharges, chargeCurrency);
						var totalChargesAmount = GetAmountTextByCurrency(sumChargesAmount, chargeCurrency);

						descriptions.Add(string.Concat(Enumerable.Repeat("-", shipmentWrapper.ChargeCodeDescColumnWidth)));
						descriptions.Add((NoResString)"Total Charges");

						if (showPrepaid && sumPrepaidCharges > 0)
						{
							prepaidAmt.Add(string.Concat(Enumerable.Repeat("-", shipmentWrapper.PrepaidChargesColumnWidth)));
							prepaidCurrency.Add("");
							prepaidAmt.Add(totalPrepaidCharges);
							prepaidCurrency.Add(totalChargeAmountCurrencyCode);
						}

						if (showCollect && sumCollectCharges > 0)
						{
							collectAmt.Add(string.Concat(Enumerable.Repeat("-", shipmentWrapper.CollectChargesColumnWidth)));
							collectCurrency.Add("");
							collectAmt.Add(totalCollectCharges);
							collectCurrency.Add(totalChargeAmountCurrencyCode);
						}

						allAmt.Add(string.Concat(Enumerable.Repeat("-", shipmentWrapper.AllChargesColumnWidth)));
						allCurrency.Add("");
						allAmt.Add(totalChargesAmount);
						allCurrency.Add(totalChargeAmountCurrencyCode);
					}
				}
			}

			var columns = new FormatColumn[]
			{
				new FormatColumn(codes, shipmentWrapper.ChargeCodeColumnWidth)
				{
					Heading = shipmentWrapper.ChargeCodeCaption,
					LeftPadding = shipmentWrapper.ChargeCodeLeftPadding,
				},

				new FormatColumn(chargeCodeDesc, shipmentWrapper.ChargeCodeDescColumnWidth)
				{
					Heading = shipmentWrapper.ChargeCodeDescCaption,
					LeftPadding = shipmentWrapper.ChargeCodeDescLeftPadding,
				},

				new FormatColumn(descriptions, shipmentWrapper.ChargeDescriptionColumnWidth)
				{
					Heading = shipmentWrapper.ChargeDescriptionCaption,
					LeftPadding = shipmentWrapper.ChargeDescriptionLeftPadding,
				},

				new FormatColumn(collectAmt, shipmentWrapper.CollectChargesColumnWidth)
				{
					Heading = showCollect ? shipmentWrapper.CollectChargesColumnCaption : ZString.Empty,
					LeftPadding = shipmentWrapper.CollectChargesColumnLeftPadding,
					Options = FormatColumnOptions.RightAlign,
				},

				new FormatColumn(collectCurrency, shipmentWrapper.CollectCurrencyColumnWidth)
				{
					Heading = showCollect ? shipmentWrapper.CollectCurrencyColumnCaption : ZString.Empty,
					LeftPadding = shipmentWrapper.CollectCurrencyColumnLeftPadding,
				},

				new FormatColumn(prepaidAmt, shipmentWrapper.PrepaidChargesColumnWidth)
				{
					Heading = showPrepaid ? shipmentWrapper.PrepaidChargesColumnCaption : ZString.Empty,
					LeftPadding = shipmentWrapper.PrepaidChargesColumnLeftPadding,
					Options = FormatColumnOptions.RightAlign,
				},

				new FormatColumn(prepaidCurrency, shipmentWrapper.PrepaidCurrencyColumnWidth)
				{
					Heading = showPrepaid ? shipmentWrapper.PrepaidCurrencyColumnCaption : ZString.Empty,
					LeftPadding = shipmentWrapper.PrepaidCurrencyColumnLeftPadding,
				},

				new FormatColumn(allAmt, shipmentWrapper.AllChargesColumnWidth)
				{
					Heading = shipmentWrapper.AllChargesColumnCaption,
					LeftPadding = shipmentWrapper.AllChargesColumnLeftPadding,
					Options = FormatColumnOptions.RightAlign,
				},

				new FormatColumn(allCurrency, shipmentWrapper.AllChargesCurrencyColumnWidth)
				{
					Heading = shipmentWrapper.AllChargesCurrencyColumnCaption,
					LeftPadding = shipmentWrapper.AllChargesCurrencyColumnLeftPadding,
				}
			};

			var indicies = new int[]
			{
				shipmentWrapper.ChargeCodeIndex,
				shipmentWrapper.ChargeCodeDescIndex,
				shipmentWrapper.ChargeDescriptionIndex,
				shipmentWrapper.CollectChargesColumnIndex,
				shipmentWrapper.CollectCurrencyColumnIndex,
				shipmentWrapper.PrepaidChargesColumnIndex,
				shipmentWrapper.PrepaidCurrencyColumnIndex,
				shipmentWrapper.AllChargesColumnIndex,
				shipmentWrapper.AllChargesCurrencyColumnIndex,
			};

			StripHiddenColumns(ref columns, ref indicies);
			Array.Sort(indicies, columns);
			return new FormatTable(shipmentWrapper.ShowChargesHeadingInMainBody, columns);
		}

		Money GetChargeAmountWithCurrency(DocJobCharge charge, RefCurrency chargeCurrency)
		{
			Money result;

			if (charge.OSSellCurrency.Code == chargeCurrency.Code)
			{
				result = new Money(charge.OSSellAmt, chargeCurrency);
			}
			else
			{
				var money = new Money(charge.OSSellAmt, charge.OSSellCurrency);
				var currencyConverter = GetCurrencyConverter();
				var chargeOSSellAmountInUSD = currencyConverter.ConvertRounded(money, chargeCurrency).Amount;
				result = new Money(chargeOSSellAmountInUSD, chargeCurrency);
			}

			return result;
		}

		internal IFormatSectionComponent NewExtraTable()
		{
			List<FormatColumn> columns = new List<FormatColumn>();

			if (shipmentWrapper.ExtraSection != null)
			{
				foreach (object[] set in shipmentWrapper.ExtraSection)
				{
					ZString heading = (string)set[0];
					ZString path = (string)set[1];
					ZInt left = (int)set[2];
					ZInt width = (int)set[3];
					ZBool hideHeadingWhenNoData = (bool)set[4];

					ZString translatedPath = ExtractValues(shipmentWrapper, path).Trim();

					columns.Add(new FormatColumn(new string[] { translatedPath }, width)
					{
						Heading = (hideHeadingWhenNoData && translatedPath.IsEmpty ? ZString.Empty : heading),
						LeftPadding = left,
					});
				}
			}

			return new FormatTable(shipmentWrapper.ShowExtraSectionHeadingInMainBody, columns.ToArray());
		}

		internal IFormatSectionComponent NewBOLClauseTable()
		{
			return NewTextTable(formedPagesSupporter.BOLClause, shipmentWrapper.BOLClauseColumnCaption, shipmentWrapper.ShowBOLClauseSectionHeadingInMainBody, shipmentWrapper.BOLClauseColumnWidth, shipmentWrapper.BOLClauseColumnLeftPadding);
		}

		internal IFormatSectionComponent NewTextTable(int width, string text)
		{
			return NewTextTable(text, string.Empty, false, width, 0);
		}

		internal IFormatSectionComponent NewTextTable(string text, string heading, bool showHeadingInMainBody, int width, int leftPadding)
		{
			FormatColumn[] columns = new FormatColumn[]
				{
					new FormatColumn(new string[] { text }, width)
					{
						Heading = heading,
						LeftPadding = leftPadding,
					}
				};

			return new FormatTable(showHeadingInMainBody, columns);
		}

		#endregion

		#region SectionSetterPairs

		SectionSetterPair[] GetSectionSetterPairs()
		{
			return new[]
			{
				new SectionSetterPair(NewMainSection(), (p,v) => { p.MainBodyDetailsSection = v; }),
				new SectionSetterPair(NewContainersSection(), (p,v) => { p.MainBodyContainersSection = v; }),
				new SectionSetterPair(NewBOLClauseSection(), (p,v) => { p.BOLClauseSection = v; }),
				new SectionSetterPair(NewRORSection(), (p,v) => { p.PackRORSection = v; }),
				new SectionSetterPair(NewChargesSection(), (p,v) => { p.ChargesSection = v; }),
				new SectionSetterPair(NewExtraSection(), (p,v) => { p.MainBodyExtraSection = v; }),
			};
		}

		class SectionSetterPair
		{
			public SectionSetterPair(FormatSection section, Action<DocBillOfLadingFormedPage, string> setter)
			{
				this.Section = section;
				this.Setter = setter;
			}

			public FormatSection Section { get; private set; }
			public Action<DocBillOfLadingFormedPage, string> Setter { get; private set; }
		}

		#endregion

		#region Sections Headers

		public ZString DetailsSectionHeader
		{
			get { return detailsSectionHeader ?? (detailsSectionHeader = GetHeaderString(NewDetailsTable())); }
		}
		string detailsSectionHeader;

		public ZString ContainersSectionHeader
		{
			get { return containersSectionHeader ?? (containersSectionHeader = GetHeaderString(NewContainersTable())); }
		}
		string containersSectionHeader;

		public ZString ExtraSectionHeader
		{
			get { return extraSectionHeader ?? (extraSectionHeader = GetHeaderString(NewExtraTable())); }
		}
		string extraSectionHeader;

		public ZString BOLClauseSectionHeader
		{
			get { return bolClauseSectionHeader ?? (bolClauseSectionHeader = GetHeaderString(NewBOLClauseTable())); }
		}
		string bolClauseSectionHeader;

		public ZString ChargesSectionHeader
		{
			get { return chargesSectionHeader ?? (chargesSectionHeader = GetHeaderString(NewChargesTable())); }
		}
		string chargesSectionHeader;

		public ZString PackRORSectionHeader
		{
			get { return packRORSectionHeader ?? (packRORSectionHeader = GetHeaderString(NewTopLevelPacksTable())); }
		}
		string packRORSectionHeader;

		#endregion

		#region Helper Methods

		string GetHeaderString(IFormatSectionComponent table)
		{
			StringBuilder builder = new StringBuilder();

			using (IEnumerator<string> enumerator = table.Heading.GetEnumerator())
			{
				if (enumerator.MoveNext())
				{
					builder.Append(enumerator.Current);

					while (enumerator.MoveNext())
					{
						builder.AppendLine();
						builder.Append(enumerator.Current);
					}
				}
			}

			return builder.ToString();
		}

		static void StripHiddenColumns(ref FormatColumn[] columns, ref int[] indicies)
		{
			int write = 0;

			for (int read = 0; read < columns.Length; read++)
			{
				int index = indicies[read];
				FormatColumn column = columns[read];

				if (column.InnerWidth > 0 && index > 0)
				{
					indicies[write] = index;
					columns[write] = column;
					write++;
				}
			}

			Array.Resize(ref columns, write);
			Array.Resize(ref indicies, write);
		}

		#region Weights and Measures

		ZDecimal ConvertWeight(ZDecimal value, ZString sourceWeightUnit, bool forceConversion = false)
		{
			return shipmentWrapper.ConvertUnits || forceConversion
				? (ZDecimal)Constants.Weight.ConvertSafe(value, sourceWeightUnit, GetTargetWeightUnit(sourceWeightUnit, forceConversion))
				: value;
		}

		ZString GetTargetWeightUnit(ZString sourceWeightUnit, bool forceConversion = false)
		{
			return shipmentWrapper.ConvertUnits || forceConversion ? shipmentWrapper.BOLWeightUnit : sourceWeightUnit;
		}

		ZDecimal ConvertVolume(ZDecimal value, ZString sourceVolumeUnit, bool forceConversion = false)
		{
			return shipmentWrapper.ConvertUnits || forceConversion
				? (ZDecimal)Constants.Volume.ConvertSafe(value, sourceVolumeUnit, GetTargetVolumeUnit(sourceVolumeUnit, forceConversion))
				: value;
		}

		ZString GetTargetVolumeUnit(ZString sourceVolumeUnit, bool forceConversion = false)
		{
			return shipmentWrapper.ConvertUnits || forceConversion ? shipmentWrapper.BOLVolumeUnit : sourceVolumeUnit;
		}

		ZDecimal ConvertLength(ZDecimal value, ZString sourceLengthUnit, bool forceConversion = false)
		{
			return shipmentWrapper.ConvertUnits || forceConversion
				? (ZDecimal)Constants.Length.ConvertSafe(value, sourceLengthUnit, GetTargetLengthUnit(sourceLengthUnit, forceConversion))
				: value;
		}

		ZString GetTargetLengthUnit(ZString sourceLengthUnit, bool forceConversion = false)
		{
			return shipmentWrapper.ConvertUnits || forceConversion ? shipmentWrapper.BOLLengthUnit : sourceLengthUnit;
		}

		ZString FormatWeightNumber(ZDecimal value)
		{
			return shipmentWrapper.FormatNumber(value, Env.Registry.WeightMinimumDecimalPlacesToDisplay);
		}

		ZString FormatVolumeNumber(ZDecimal value)
		{
			if (value.IsEmpty)
			{
				return ZString.Empty;
			}
			else
			{
				return shipmentWrapper.FormatNumber(value, Env.Registry.VolumeMinimumDecimalPlacesToDisplay);
			}
		}

		ZString FormatDimensions(decimal volume, ZString volumeUnit, decimal length, decimal width, decimal height, ZString dimensionsUnit)
		{
			ZString dimensionsWithUnits = ZString.Empty;

			var dimensionsBuilder = new ZStringBuilder();
			if (length != 0)
			{
				dimensionsBuilder.Append(shipmentWrapper.FormatNumber(length, shipmentWrapper.DimensionsDecimalPlaces));
			}

			if (width != 0)
			{
				dimensionsBuilder.Append(shipmentWrapper.FormatNumber(width, shipmentWrapper.DimensionsDecimalPlaces));
			}

			if (height != 0)
			{
				dimensionsBuilder.Append(shipmentWrapper.FormatNumber(height, shipmentWrapper.DimensionsDecimalPlaces));
			}

			if (!dimensionsBuilder.IsEmpty && !dimensionsUnit.IsEmpty)
			{
				dimensionsWithUnits = dimensionsBuilder.ToStringWithDelimiterBetweenAppends((NoResString)"x") + " " + dimensionsUnit;
			}

			var volumeAndDimensionsBuilder = new ZStringBuilder();
			volumeAndDimensionsBuilder.AppendIfNotEmpty(FormatVolumeNumber(volume));

			if (!volumeAndDimensionsBuilder.IsEmpty)
			{
				volumeAndDimensionsBuilder.AppendIfNotEmpty(volumeUnit);
			}

			volumeAndDimensionsBuilder.AppendIfNotEmpty(dimensionsWithUnits);

			return volumeAndDimensionsBuilder.ToStringWithDelimiterBetweenAppends(" ");
		}

		#endregion

		#region Vehicle Details

		ZString FormatVehicleFullDetails(ZString vehicleColour, ZString vehicleMake, ZString vehicleModel, ZByte vehicleNumberOfDoors,
						ZString vehicleTransmission, ZShort vehicleYear, ZString detailedDescription, ZString undgs)
		{
			var builder = new ZStringBuilder();
			var vehicleDetails = new List<ZString>();

			if (vehicleYear != 0)
			{
				vehicleDetails.Add(vehicleYear.ToString());
			}

			if (!vehicleMake.IsEmpty)
			{
				vehicleDetails.Add(vehicleMake);
			}

			if (!vehicleModel.IsEmpty)
			{
				vehicleDetails.Add(vehicleModel);
			}

			if (!vehicleColour.IsEmpty)
			{
				vehicleDetails.Add(vehicleColour);
			}

			if (vehicleNumberOfDoors != 0)
			{
				vehicleDetails.Add(vehicleNumberOfDoors.ToString() + " " + Res.GetString("789d6e9b-9558-4736-a03e-6147fb429e94", "door"));
			}

			if (!vehicleTransmission.IsEmpty)
			{
				vehicleDetails.Add(vehicleTransmission);
			}

			if (vehicleDetails.Count > 0)
			{
				builder.Append(ZString.Join(" ", vehicleDetails.ToArray()));
			}

			if (!detailedDescription.IsEmpty)
			{
				builder.Append(detailedDescription);
			}

			if (!undgs.IsEmpty)
			{
				builder.Append(undgs);
			}

			return builder.ToStringWithNewLineBetweenAppends();
		}

		#endregion

		ZString HazardousDescription(UNDGSubstanceWrapper[] uNDGs, bool includeEmergencyContact)
		{
			var builder = new ZStringBuilder();

			foreach (UNDGSubstanceWrapper undg in uNDGs)
			{
				var undgSummaryBuilder = new ZStringBuilder();
				undgSummaryBuilder.AppendIfNotEmpty(undg.Summary);
				undgSummaryBuilder.AppendIfNotEmpty((new ExclusiveUseComponent() as IUNDGSummaryWriterComponent).Write(undg));
				builder.AppendIfNotEmpty(undgSummaryBuilder.ToStringWithDelimiterBetweenAppends(", "));

				if (includeEmergencyContact && undg.DGContact != null && !undg.DGContact.FullName.IsEmpty)
				{
					var contactBuilder = new ZStringBuilder((NoResString)"EMERGENCY CONTACT: " + undg.DGContact.FullName);
					if (!undg.DGContact.Phone.IsEmpty)
					{
						contactBuilder.Append((NoResString)"phone: " + undg.DGContact.Phone);
					}

					builder.Append(contactBuilder.ToStringWithDelimiterBetweenAppends(", "));
				}
			}

			var helper = new UNDGSubstanceWrapperHelper();
			var summary = helper.GetUNDGPackagesSummary(uNDGs);
			builder.AppendIfNotEmpty(summary);

			return builder.ToStringWithNewLineBetweenAppends();
		}

		static string ExtractValue(object source, string pathStr)
		{
			string[] path = pathStr.Split(new char[] { '.' }, StringSplitOptions.RemoveEmptyEntries);

			object step = source;
			foreach (string segment in path)
			{
				if (step == null)
				{
					return null;
				}

				PropertyInfo info = step.GetType().GetProperty(segment);
				step = info == null ? null : info.GetValue(step, null);
			}

			return step == null ? null : step.ToString();
		}

		static string ExtractValues(object source, string format)
		{
			StringBuilder result = new StringBuilder();

			for (int i = 0; i < format.Length; i++)
			{
				char c = format[i];

				if (c == '\\')
				{
					i++;
					if (i < format.Length)
					{
						result.Append(format[i]);
					}
				}
				else if (c == '<')
				{
					int end = format.IndexOf('>', i);
					if (end < 0)
					{
						break;
					}
					else
					{
						result.Append(ExtractValue(source, format.Substring(i + 1, end - i - 1)));
						i = end;
					}
				}
				else
				{
					result.Append(format[i]);
				}
			}

			return result.ToString();
		}

		ZBool IsCollect(DocJobCharge charge)
		{
			return formedPagesSupporter.CollectCharges.Cast<DocJobCharge>().Any(x => x.JobCharge != null && x.JobCharge.PK == charge.JobCharge.PK);
		}

		ZBool IsPrepaid(DocJobCharge charge)
		{
			return !IsCollect(charge);
		}

		ZString GetAmountTextByCurrency(ZDecimal amount, ICurrency currency)
		{
			var result = new Money(amount, currency).ToString();
			return result.Split(' ')[0];
		}

		#endregion
	}
}
