using System;
using CargoWise.Types;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Customs.EU.TemporaryStorage.GUI
{
	public sealed class UCC6TemporaryStorageAdditionalInformationDetailsLayoutWithGrid : IPanelLayoutWithGridProvider
	{
		public UCC6TemporaryStorageAdditionalInformationDetailsLayoutWithGrid() : this(UCC6TemporaryStorageAdditionalInfosUserControlWithGrid.UCC6TemporaryStorageBillPackedItemAdditionalInfoBingdingMemberName)
		{
		}

		public UCC6TemporaryStorageAdditionalInformationDetailsLayoutWithGrid(ZString dataMember)
		{
			this.dataMember = dataMember;
		}
		readonly ZString dataMember;

		public Type GridUserControlType => typeof(UCC6TemporaryStorageAdditionalInfosUserControlWithGrid);

		PanelLayout IPanelLayoutProvider.Layout => CreateLayout();

		PanelLayout CreateLayout()
		{
			var builder = new UCC6TemporaryStorageAdditionalInformationDetailsLayoutBuilder();
			var euBag = builder.CommonBag;
			builder.AddColumn();

			switch (dataMember)
			{
				case UCC6TemporaryStorageAdditionalInfosUserControlWithGrid.UCC6TemporaryStorageBillPackedItemAdditionalInfoBingdingMemberName:
					builder.Add(euBag.KindDropEdit, ControlWidthClass.Long);
					builder.Add(euBag.FullTypeCodeFindBox, ControlWidthClass.Long);
					builder.Add(euBag.ReferenceTextBox, ControlWidthClass.Long);
					builder.Add(euBag.DescriptionTextBox, ControlWidthClass.Long);
					break;
				case UCC6TemporaryStorageAdditionalInfosUserControlWithGrid.UCC6TemporaryStorageBillAdditionalInfoBingdingMemberName:
					builder.Add(euBag.ReferenceTextBox, ControlWidthClass.Long);
					break;
			}

			return builder.Build();
		}
	}
}
