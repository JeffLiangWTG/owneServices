using System.ComponentModel;
using System.Data;
using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using CargoWise.Types;
using CargoWiseOne.ResourceStrings;
using Enterprise.Customs.Business;

namespace Enterprise.Customs.EU.Manifest.ICS2.Business
{
	public class CusStorageDocPivot : BaseCusStorageDocPivot
	{
		public CusStorageDocPivot(BusinessObjectFactory factory, DataRow row) : base(factory, row)
		{
		}

		protected override Customs.Business.CusStorageDocPivotLookups GetNewLookups()
		{
			return new CusStorageDocPivotLookups(this);
		}

		public new CusStorageDocPivotLookups Lookups => (CusStorageDocPivotLookups)base.Lookups;

		protected override TypeLoaderCollection parentLoaders
		{
			get
			{
				var result = new TypeLoaderCollection();

				result.Add(typeof(AsycudaManifestHeader));
				result.Add(typeof(AsycudaBill));
				result.Add(typeof(RequestHeader));

				return result;
			}
		}

		[List(nameof(Lookups) + "." + nameof(CusStorageDocPivotLookups.AvailableEDocs), "PK", "Code", AllowOnlyTheseValues = true)]
		[ResourceStringData("Enterprise.Customs.EU.Manifest.ICS2.Business.CusStorageDocPivot.CSD_StorageDocReference", Caption = "File Name")]
		public override ZGuid CSD_StorageDocReference
		{
			get => base.CSD_StorageDocReference;
			set
			{
				if (CSD_StorageDocReference != value)
				{
					ResetDocument();
					base.CSD_StorageDocReference = value;

					if (!IsCopying)
					{
						CSD_DocType = Document?.DataType ?? ZString.Empty;

						if (CSD_Description.IsEmpty)
						{
							CSD_Description = Document != null ? Document.Description : ZString.Empty;
						}
					}
				}
			}
		}

		[ResourceStringData("Enterprise.Customs.EU.Manifest.ICS2.Business.CusStorageDocPivot.CSD_Description", Caption = "Description")]
		public override ZString CSD_Description { get => base.CSD_Description; set => base.CSD_Description = value; }

		[ResourceStringData("Enterprise.Customs.EU.Manifest.ICS2.Business.CusStorageDocPivot.CSD_DocType", Caption = "File Type")]
		[ReadOnly(true)]
		public override ZString CSD_DocType { get => base.CSD_DocType; set => base.CSD_DocType = value; }

		public new CusStorageDocPivotValidation Validation => (CusStorageDocPivotValidation)base.Validation;

		protected override Customs.Business.CusStorageDocPivotValidation GetNewValidation() => new CusStorageDocPivotValidation(this);
	}
}
