using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Res = DocumentWrappers.Res;

namespace Enterprise.DocumentWrappers
{
	public abstract class FormatterForBillTypeLayout : NonPersistentBusinessObject, IObsoleteValidation
	{
		public FormatterForBillTypeLayout()
		{
			HaveCalculatedContainerColumns = false;
			HaveCalculatedDescription = false;
			HaveCalculatedMarksAndNumbers = false;
			fGoodDetails = ZString.Empty;
		}

		#region Abstract

		public abstract ZString UnformattedMarksAndNumbers { get; }
		public abstract ZString UnformattedGoodsDescription { get; }
		public abstract ZInt PackCount { get; }
		public abstract ZString PackageTypeDescription { get; }
		public abstract IDocSimpleContainerCollection Containers { get; }
		public abstract ZString ContainerSectionHeading { get; }
		public abstract ZBool ShowContainerAdditionalDetails { get; }

		public abstract ZInt MarksAndNumbersAndDescriptionRowHeight { get; }
		public abstract ZInt MarksAndNumbersWidth { get; }
		public abstract ZInt DescriptionWidth { get; }
		public abstract ZInt ContainerRowHeight { get; }

		#endregion

		#region Virtual

		public virtual ZString UnformattedWeight
		{
			get { return ZString.Empty; }
		}

		public virtual ZInt WeightWidth
		{
			get { return 0; }
		}

		public virtual ZString UnformattedVolume
		{
			get { return ZString.Empty; }
		}

		public virtual ZInt VolumeWidth
		{
			get { return 0; }
		}

		public virtual ZString DeliveryModeForContainer(IDocSimpleContainer container)
		{
			return ZString.Empty;
		}

		#endregion

		#region Good Details

		public ZString GoodDetails
		{
			get
			{
				if (fGoodDetails.IsEmpty)
				{
					ZString[] spiltMarksAndNumbers = MarksAndNumbers.Split('\n');
					ZInt lineCountInMarksAndNumbers = spiltMarksAndNumbers.Length;

					ZString[] spiltGoodsDescription = GoodsDescription.Split('\n');
					ZInt lineCountInGoodsDescription = spiltGoodsDescription.Length;

					ZInt goodDetailsHeight = 0;

					if (lineCountInMarksAndNumbers < MarksAndNumbersAndDescriptionRowHeight
						&& lineCountInGoodsDescription < MarksAndNumbersAndDescriptionRowHeight)
					{
						if (lineCountInMarksAndNumbers > lineCountInGoodsDescription)
						{
							goodDetailsHeight = lineCountInMarksAndNumbers;
							fContainerHeightForGoodDetails = (MarksAndNumbersAndDescriptionRowHeight + ContainerRowHeight + 1) - lineCountInMarksAndNumbers;
						}
						else
						{
							goodDetailsHeight = lineCountInGoodsDescription;
							fContainerHeightForGoodDetails = (MarksAndNumbersAndDescriptionRowHeight + ContainerRowHeight + 1) - lineCountInGoodsDescription;
						}
					}
					else
					{
						goodDetailsHeight = MarksAndNumbersAndDescriptionRowHeight;
						fContainerHeightForGoodDetails = ContainerRowHeight;
					}

					BuildContainerColumns();

					ZString[] spiltWeight = Weight.Split('\n');
					ZString[] spiltVolume = Volume.Split('\n');

					for (int i = 0; i < goodDetailsHeight; i++)
					{
						fGoodDetails += (i < lineCountInMarksAndNumbers) ? spiltMarksAndNumbers[i] + " " : string.Empty.PadRight(MarksAndNumbersWidth + 1);
						fGoodDetails += (i < lineCountInGoodsDescription) ? spiltGoodsDescription[i] + " " : string.Empty.PadRight(DescriptionWidth + 1);
						fGoodDetails += (i < spiltWeight.Length) ? spiltWeight[i] + " " : string.Empty.PadRight(WeightWidth + 1);
						fGoodDetails += (i < spiltVolume.Length) ? spiltVolume[i] + "\n" : string.Empty.PadRight(VolumeWidth) + "\n";
					}

					if (!ContainerNumberColumn.IsEmpty)
					{
						fGoodDetails += ContainerSectionHeading + "\n";
					}

					ZString[] spiltContainerNumber = ContainerNumberColumn.Split('\n');
					ZString[] spiltSealNumber = ContainerSealNumColumn.Split('\n');
					ZString[] spiltContainerType = ContainerTypeColumn.Split('\n');
					ZString[] spiltContainerWeight = ContainerWeightColumn.Split('\n');
					ZString[] spiltContainerVolume = ContainerVolumeColumn.Split('\n');
					ZString[] spiltContainerPackage = ContainerPackagesColumn.Split('\n');
					ZString[] spiltContainerMode = ContainerModeColumn.Split('\n');

					for (int i = 0; i < spiltContainerNumber.Length; i++)
					{
						if (i < spiltContainerNumber.Length)
						{
							fGoodDetails += spiltContainerNumber[i] + " ";
						}

						if (i < spiltSealNumber.Length)
						{
							fGoodDetails += spiltSealNumber[i] + " ";
						}

						if (i < spiltContainerType.Length)
						{
							fGoodDetails += spiltContainerType[i] + " ";
						}

						if (ShowContainerAdditionalDetails)
						{
							if (i < spiltContainerWeight.Length)
							{
								fGoodDetails += spiltContainerWeight[i] + " ";
							}

							if (i < spiltContainerVolume.Length)
							{
								fGoodDetails += spiltContainerVolume[i] + " ";
							}

							if (i < spiltContainerPackage.Length)
							{
								fGoodDetails += spiltContainerPackage[i] + " ";
							}

							if (i < spiltContainerMode.Length)
							{
								fGoodDetails += spiltContainerMode[i];
							}
						}

						fGoodDetails += "\n";
					}
				}

				return fGoodDetails.Trim('\n');
			}
		}

		protected ZString fGoodDetails;
		protected ZInt fContainerHeightForGoodDetails;
		#endregion

		#region MarksAndNumbers

		public ZString MarksAndNumbers
		{
			get
			{
				BuildMarksAndNumbers();
				return fMarksAndNumbers.TrimEnd('\n');
			}
		}

		public ZString FollowOnMarksAndNumbers
		{
			get
			{
				BuildMarksAndNumbers();
				return fFollowOnMarksAndNumbers.Trim();
			}
		}

		protected ZString fMarksAndNumbers;
		protected ZString fFollowOnMarksAndNumbers;

		#endregion

		#region Goods Description

		public ZString GoodsDescription
		{
			get
			{
				BuildDescription();
				return fDescription.TrimEnd('\n');
			}
		}

		public ZString FollowOnGoodsDescription
		{
			get
			{
				BuildDescription();
				return fFollowOnDescription.Trim();
			}
		}

		protected ZString fDescription;
		protected ZString fFollowOnDescription;

		#endregion

		#region Weight

		public ZString Weight
		{
			get
			{
				if (WeightWidth != 0)
				{
					if (fWeight.IsEmpty)
					{
						fWeight = WrapTextForAColumn(UnformattedWeight, WeightWidth);
					}
					return fWeight;
				}

				return UnformattedWeight;
			}
		}

		protected ZString fWeight;
		#endregion

		#region Volume

		public ZString Volume
		{
			get
			{
				if (VolumeWidth != 0)
				{
					if (fVolume.IsEmpty)
					{
						fVolume = WrapTextForAColumn(UnformattedVolume, VolumeWidth);
					}
					return fVolume;
				}

				return UnformattedVolume;
			}
		}

		protected ZString fVolume;

		#endregion

		#region LCL Packages

		public ZInt CountOfLCLPackages
		{
			get { return fCountOfLCLPackages; }
		}

		protected ZInt fCountOfLCLPackages;

		#endregion

		#region Container Number

		public ZString ContainerNumberColumn
		{
			get
			{
				BuildContainerColumns();
				return fContainerNumberColumn.TrimEnd('\n');
			}
		}

		public ZString FollowOnContainerNumberColumn
		{
			get
			{
				BuildContainerColumns();
				return fFollowOnContainerNumberColumn.TrimEnd('\n');
			}
		}

		protected ZString fContainerNumberColumn;
		protected ZString fFollowOnContainerNumberColumn;
		protected const int ContainerNumberWidth = 16;

		#endregion

		#region Container Seal Number

		public ZString ContainerSealNumColumn
		{
			get
			{
				BuildContainerColumns();
				return fContainerSealNumColumn.TrimEnd('\n');
			}
		}

		public ZString FollowOnContainerSealNumColumn
		{
			get
			{
				BuildContainerColumns();
				return fFollowOnContainerSealNumColumn.TrimEnd('\n');
			}
		}

		protected ZString fContainerSealNumColumn;
		protected ZString fFollowOnContainerSealNumColumn;
		protected const int ContainerSealWidth = 24;

		#endregion

		#region Container Type

		public ZString ContainerTypeColumn
		{
			get
			{
				BuildContainerColumns();
				return fContainerTypeColumn.TrimEnd('\n');
			}
		}

		public ZString FollowOnContainerTypeColumn
		{
			get
			{
				BuildContainerColumns();
				return fFollowOnContainerTypeColumn.TrimEnd('\n');
			}
		}

		protected ZString fContainerTypeColumn;
		protected ZString fFollowOnContainerTypeColumn;
		protected const int ContainerTypeWidth = 12;

		#endregion

		#region Container Weight

		public ZString ContainerWeightColumn
		{
			get
			{
				BuildContainerColumns();
				return fContainerWeightColumn.TrimEnd('\n');
			}
		}

		public ZString FollowOnContainerWeightColumn
		{
			get
			{
				BuildContainerColumns();
				return fFollowOnContainerWeightColumn.TrimEnd('\n');
			}
		}

		protected ZString fContainerWeightColumn;
		protected ZString fFollowOnContainerWeightColumn;
		protected const int ContainerWeightWidth = 15;

		#endregion

		#region Container Volume

		public ZString ContainerVolumeColumn
		{
			get
			{
				BuildContainerColumns();
				return fContainerVolumeColumn.TrimEnd('\n');
			}
		}

		public ZString FollowOnContainerVolumeColumn
		{
			get
			{
				BuildContainerColumns();
				return fFollowOnContainerVolumeColumn.TrimEnd('\n');
			}
		}

		protected ZString fContainerVolumeColumn;
		protected ZString fFollowOnContainerVolumeColumn;
		protected const int ContainerVolumeWidth = 20;

		#endregion

		#region Container Packages

		public ZString ContainerPackagesColumn
		{
			get
			{
				BuildContainerColumns();
				return fContainerPackagesColumn.TrimEnd('\n');
			}
		}

		public ZString FollowOnContainerPackagesColumn
		{
			get
			{
				BuildContainerColumns();
				return fFollowOnContainerPackagesColumn.TrimEnd('\n');
			}
		}

		protected ZString fContainerPackagesColumn;
		protected ZString fFollowOnContainerPackagesColumn;
		protected const int ContainerPackagesWidth = 14;

		#endregion

		#region Container Mode

		public ZString ContainerModeColumn
		{
			get
			{
				BuildContainerColumns();
				return fContainerModeColumn.TrimEnd('\n');
			}
		}

		public ZString FollowOnContainerModeColumn
		{
			get
			{
				BuildContainerColumns();
				return fFollowOnContainerModeColumn.TrimEnd('\n');
			}
		}

		protected ZString fContainerModeColumn;
		protected ZString fFollowOnContainerModeColumn;
		protected const int ContainerModeWidth = 12;

		#endregion

		#region Implementation

		protected ZBool HaveCalculatedMarksAndNumbers;
		protected ZBool HaveCalculatedDescription;
		protected ZBool HaveCalculatedContainerColumns;
		protected BusinessObjectFactory fFactory;
		protected ZString fContainerCount;
		protected ZString fPackCountAndDescription;

		protected void BuildMarksAndNumbers()
		{
			if (!HaveCalculatedMarksAndNumbers)
			{
				ZString containerAndPacakages = ContainerCount + PackCountAndDescription;
				ZInt numberOfNewLines = containerAndPacakages.Split('\n').Length;

				ZString newLineString = ZString.Empty;
				for (int i = 0; i < numberOfNewLines - 1; i++)
				{
					newLineString += "\n";
				}

				ZString formattedMarksAndNumber = WrapTextForAColumn(newLineString + UnformattedMarksAndNumbers, MarksAndNumbersWidth);

				ZString[] marksSplitByRow = formattedMarksAndNumber.Split('\n');

				for (int i = 0; i < MarksAndNumbersAndDescriptionRowHeight; i++)
				{
					if (i < marksSplitByRow.Length)
					{
						fMarksAndNumbers += marksSplitByRow[i] + "\n";
					}
				}

				for (int i = MarksAndNumbersAndDescriptionRowHeight; i < marksSplitByRow.Length; i++)
				{
					fFollowOnMarksAndNumbers += marksSplitByRow[i].Trim() + " ";
				}

				HaveCalculatedMarksAndNumbers = ZBool.True;
			}
		}

		protected void BuildDescription()
		{
			if (!HaveCalculatedDescription)
			{
				ZString unformattedDescription = ContainerCount + PackCountAndDescription + UnformattedGoodsDescription;
				ZString formattedDescription = WrapTextForAColumn(unformattedDescription, DescriptionWidth);

				if (!formattedDescription.IsEmpty)
				{
					ZString[] descriptionSplitByRow = formattedDescription.Split('\n');

					for (int i = 0; i < MarksAndNumbersAndDescriptionRowHeight; i++)
					{
						if (i < descriptionSplitByRow.Length)
						{
							fDescription += descriptionSplitByRow[i] + "\n";
						}
					}

					for (int i = MarksAndNumbersAndDescriptionRowHeight; i < descriptionSplitByRow.Length; i++)
					{
						fFollowOnDescription += descriptionSplitByRow[i].Trim() + " ";
					}
				}

				HaveCalculatedDescription = ZBool.True;
			}
		}

		protected void BuildContainerColumns()
		{
			if (!HaveCalculatedContainerColumns)
			{
				ZInt rowHeight = (fContainerHeightForGoodDetails.IsEmpty) ? ContainerRowHeight : fContainerHeightForGoodDetails;

				IDocSimpleContainerCollection currentContainers = Containers;
				currentContainers.Sort("ContainerNumber", System.ComponentModel.ListSortDirection.Ascending);

				ZString formattedContainerNumbers = ZString.Empty;
				ZString formattedSealNumbers = ZString.Empty;
				ZString formattedType = ZString.Empty;
				ZString formattedWeight = ZString.Empty;
				ZString formattedVolume = ZString.Empty;
				ZString formattedPackages = ZString.Empty;
				ZString formattedDeliveryMode = ZString.Empty;

				foreach (IDocSimpleContainer currentContainer in currentContainers)
				{
					formattedContainerNumbers += currentContainer.ContainerNumber.PadRight(ContainerNumberWidth) + "\n";

					if (currentContainer.SealNumber.IsEmpty)
					{
						formattedSealNumbers += "-".PadRight(ContainerSealWidth) + "\n";
					}
					else
					{
						formattedSealNumbers += currentContainer.SealNumber.PadRight(ContainerSealWidth) + "\n";
					}

					if (currentContainer.Container != null && !currentContainer.Container.Code.IsEmpty)
					{
						formattedType += currentContainer.Container.Code.PadRight(ContainerTypeWidth) + "\n";
					}
					else
					{
						formattedType += "-".PadRight(ContainerTypeWidth) + "\n";
					}

					if (ShowContainerAdditionalDetails)
					{
						if (currentContainer.TotalAllocatedJobWeight.IsEmpty)
						{
							formattedWeight += "-".PadRight(ContainerWeightWidth) + "\n";
						}
						else
						{
							formattedWeight += currentContainer.TotalAllocatedJobWeight.ToString().PadRight(ContainerWeightWidth) + "\n";
						}

						if (currentContainer.TotalAllocatedJobVolume.IsEmpty)
						{
							formattedVolume += "-".PadRight(ContainerVolumeWidth) + "\n";
						}
						else
						{
							formattedVolume += currentContainer.TotalAllocatedJobVolume.ToString().PadRight(ContainerVolumeWidth) + "\n";
						}

						if (currentContainer.TotalAllocatedJobPackages.IsEmpty)
						{
							formattedPackages += "-".PadRight(ContainerPackagesWidth) + "\n";
						}
						else
						{
							formattedPackages += currentContainer.TotalAllocatedJobPackages.ToString().PadRight(ContainerPackagesWidth) + "\n";
						}

						if (currentContainer.DeliveryMode.IsEmpty)
						{
							formattedDeliveryMode += "-".PadRight(ContainerModeWidth) + "\n";
						}
						else
						{
							formattedDeliveryMode += currentContainer.DeliveryMode.PadRight(ContainerModeWidth) + "\n";
						}
					}
				}

				if (!formattedContainerNumbers.IsEmpty)
				{
					ZString[] containerNumbersSpiltByRow = formattedContainerNumbers.Split('\n');
					ZString[] sealNumbersSpiltByRow = formattedSealNumbers.Split('\n');
					ZString[] typeSpiltByRow = formattedType.Split('\n');
					ZString[] weightSpiltByRow = formattedWeight.Split('\n');
					ZString[] volumeSpiltByRow = formattedVolume.Split('\n');
					ZString[] packagesSpiltByRow = formattedPackages.Split('\n');
					ZString[] deliveryModeSpiltByRow = formattedDeliveryMode.Split('\n');

					for (int i = 0; i < rowHeight; i++)
					{
						if (i < containerNumbersSpiltByRow.Length)
						{
							fContainerNumberColumn += containerNumbersSpiltByRow[i] + "\n";
							fContainerSealNumColumn += sealNumbersSpiltByRow[i] + "\n";
							fContainerTypeColumn += typeSpiltByRow[i] + "\n";

							if (ShowContainerAdditionalDetails)
							{
								fContainerWeightColumn += weightSpiltByRow[i] + "\n";
								fContainerVolumeColumn += volumeSpiltByRow[i] + "\n";
								fContainerPackagesColumn += packagesSpiltByRow[i] + "\n";
								fContainerModeColumn += deliveryModeSpiltByRow[i] + "\n";
							}
						}
					}

					for (int i = rowHeight; i < containerNumbersSpiltByRow.Length; i++)
					{
						fFollowOnContainerNumberColumn += containerNumbersSpiltByRow[i] + "\n";
						fFollowOnContainerSealNumColumn += sealNumbersSpiltByRow[i] + "\n";
						fFollowOnContainerTypeColumn += typeSpiltByRow[i] + "\n";

						if (ShowContainerAdditionalDetails)
						{
							fFollowOnContainerWeightColumn += weightSpiltByRow[i] + "\n";
							fFollowOnContainerVolumeColumn += volumeSpiltByRow[i] + "\n";
							fFollowOnContainerPackagesColumn += packagesSpiltByRow[i] + "\n";
							fFollowOnContainerModeColumn += deliveryModeSpiltByRow[i] + "\n";
						}
					}
				}

				HaveCalculatedContainerColumns = ZBool.True;
			}
		}

		protected ZString ContainerCount
		{
			get
			{
				if (fContainerCount.IsEmpty)
				{
					fContainerCount = string.Join("",
						Containers
						.OfType<IDocSimpleContainer>()
						.Where(currentContainer => currentContainer.Type == "FCL" && currentContainer.Container != null)
						.GroupBy(currentContainer => currentContainer.Container.Code)
						.OrderByDescending(x => x.Count())
						.Select(group => Res.GetString("0eaab763-6716-4e47-8ace-c66e9f01e7cc", "{0} X {1} CONTAINER(S)", group.Count(), group.Key) + "\n"));
				}

				return fContainerCount;
			}
		}

		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1161:Res.GetString Analyzer", Justification = "Description ZString")]
		protected ZString PackCountAndDescription
		{
			get
			{
				if (fPackCountAndDescription.IsEmpty)
				{
					ZInt fCLPackageCount = 0;
					foreach (IDocSimpleContainer currentContainer in Containers)
					{
						if (currentContainer.Type == "FCL")
						{
							fCLPackageCount += currentContainer.TotalAllocatedJobPackages;
						}
					}

					if (fCLPackageCount.IsEmpty)
					{
						fCountOfLCLPackages = PackCount;
						if (!PackCount.IsEmpty)
						{
							fPackCountAndDescription = PackCount + " " + PackageTypeDescription + "(s)\n";
						}
					}
					else if (PackCount > fCLPackageCount)
					{
						ZInt packageDifference = PackCount - fCLPackageCount;
						fCountOfLCLPackages = packageDifference;
						fPackCountAndDescription = Res.GetString("d3c7ccd5-b12b-4d43-af92-f191a828ea72", "STC {0} {1}(s)", fCLPackageCount, PackageTypeDescription) + "\n";
						if (!packageDifference.IsEmpty)
						{
							fPackCountAndDescription += Res.GetString("7ad994f4-3b10-483e-a3a8-c76a12601db6", "and {0} {1}(s) LCL Cargo", packageDifference, PackageTypeDescription) + "\n";
						}
					}
					else if (fCLPackageCount >= PackCount)
					{
						if (!PackCount.IsEmpty)
						{
							fPackCountAndDescription = Res.GetString("d3c7ccd5-b12b-4d43-af92-f191a828ea72", "STC {0} {1}(s)", PackCount, PackageTypeDescription) + "\n";
						}
					}
				}
				return fPackCountAndDescription;
			}
		}

		protected ZString WrapTextForAColumn(ZString value, ZInt columnWidth)
		{
			ZString result = ZString.Empty;
			ZString columnText = ZString.Empty;
			ZString stringValue = value.Replace("\n", " \n ");

			foreach (ZString currentWord in stringValue.Split(' '))
			{
				if (currentWord == "\n")
				{
					result += columnText.PadRight(columnWidth, ' ') + "\n";
					columnText = ZString.Empty;
				}
				else if (columnText.Length + currentWord.Length == columnWidth)
				{
					columnText += currentWord;
					result += columnText.PadRight(columnWidth, ' ') + "\n";
					columnText = ZString.Empty;
				}
				else if (columnText.Length + currentWord.Length + 1 > columnWidth)
				{
					result += columnText.PadRight(columnWidth, ' ') + "\n";
					columnText = currentWord + " ";
				}
				else
				{
					columnText += currentWord + " ";
				}
			}

			if (!columnText.IsEmpty)
			{
				result += columnText.PadRight(columnWidth, ' ');
			}

			return result;
		}

		#endregion
	}
}
