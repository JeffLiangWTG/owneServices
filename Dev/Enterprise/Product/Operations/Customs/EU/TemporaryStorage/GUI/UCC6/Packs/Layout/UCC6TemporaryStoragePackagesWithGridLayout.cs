using System;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Customs.EU.TemporaryStorage.GUI
{
	public class UCC6TemporaryStoragePackagesWithGridLayout : IPanelLayoutWithGridProvider
	{
		public UCC6TemporaryStoragePackagesWithGridLayout()
		{
			PackageDetailLayout = CreateUCC6TemporaryStoragePackageLayout();
		}

		public Type GridUserControlType => typeof(UCC6TemporaryStoragePackagesGridControl);

		PanelLayout PackageDetailLayout { get; }

		public PanelLayout Layout => PackageDetailLayout;

		PanelLayout CreateUCC6TemporaryStoragePackageLayout()
		{
			var builder = new UCC6TemporaryStoragePackageDetailsBuilder();
			var commonBag = builder.CommonBag;

			builder.AddColumn();
			builder.Add(commonBag.ContainerDropEdit, ControlWidthClass.Long);
			builder.Add(commonBag.PackQtyCalcEdit, ControlWidthClass.Long);
			builder.Add(commonBag.PackUQDropEdit, ControlWidthClass.Long);
			builder.Add(commonBag.MarksAndNumberTextBox, ControlWidthClass.Long);

			builder.SetVisibility(commonBag.ContainerDropEdit, x => !x.IsTransfer, x => x.AMA_MessageTypeInfo);
			builder.SetVisibility(commonBag.MarksAndNumberTextBox, x => !x.IsTransfer, x => x.AMA_MessageTypeInfo);

			return builder.Build();
		}
	}
}
