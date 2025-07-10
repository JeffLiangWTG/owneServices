using System;
using CargoWise.EntityFramework;
using CargoWise.Types;

namespace Enterprise.Customs.AU.Declaration.Business
{
	public sealed class TemporaryManifest : NonPersistentBusinessObject, IObsoleteValidation
	{
		public TemporaryManifest(BusinessObjectFactory factory, CusSeaManTranHead manifest)
			: base(factory)
		{
			if (manifest == null)
			{
				throw new ArgumentNullException(nameof(manifest));
			}

			manifest.SetReadOnlyIncludingChildren(true);
			this.importManifest = manifest;
		}

		public TemporaryManifest(BusinessObjectFactory factory, CalcExportManifestHeader manifest)
			: base(factory)
		{
			if (manifest == null)
			{
				throw new ArgumentNullException(nameof(manifest));
			}

			manifest.SetReadOnlyIncludingChildren(true);
			this.calcExportManifest = manifest;
		}

		#region Schema

		public abstract class Schema
		{
			public const string ShouldSave = "ShouldSave";
		}

		#endregion

		#region ShouldSave

		public ZBool ShouldSave
		{
			get { return shouldSave; }
			set
			{
				SetNonPersistentPropertyValue(ShouldSaveInfo, ref shouldSave, value);
				ValidateShouldSave();
			}
		}
		ZBool shouldSave;

		public ZPropertyInfo ShouldSaveInfo
		{
			get { return GetZPropertyInfo(Schema.ShouldSave); }
		}

		void ValidateShouldSave()
		{
			ShouldSaveInfo.ClearAllNotifications();

			if (shouldSave && IsDuplicated)
			{
				ShouldSaveInfo.AddError("This manifest is duplicated and can not be selected for update.");
			}
		}

		#endregion

		public ZBool IsDuplicated { get; set; }

		#region Manifests

		public ExportCustomsManifestHeader ExportManifest { get; set; }

		public CusSeaManTranHead ImportManifest
		{
			[System.Diagnostics.DebuggerStepThrough]
			get { return importManifest; }
		}
		readonly CusSeaManTranHead importManifest;

		public CalcExportManifestHeader CalcExportManifest
		{
			get { return calcExportManifest; }
		}
		readonly CalcExportManifestHeader calcExportManifest;

		#endregion

		#region BusinessObject Overrides

		protected override void SetDefaultValues()
		{
			base.SetDefaultValues();

			ShouldSave = true;
		}

		#endregion
	}
}
