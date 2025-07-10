using System.ComponentModel;
using System.Data;
using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Client.EDI.LicenceKeyBuilder.Business
{
	[ProvideMetaDataProperty("ReadOnlySecurity", MetaDataTypes.ReadOnly)]
	public class Licence3rdPartySoftware : AutoLicence3rdPartySoftware
	{
		public Licence3rdPartySoftware(BusinessObjectFactory factory, DataRow row)
			: base(factory, row)
		{
		}

		public new class Schema : AutoLicence3rdPartySoftware.Schema
		{
			public const string PartDescription = "PartDescription";
		}

		#region Overrides

		[List("Lookups.Products")]
		public override ZGuid L3_OP_ProductSKU
		{
			get { return base.L3_OP_ProductSKU; }
			set
			{
				base.L3_OP_ProductSKU = value;
				if (L3_OP_ProductSKU.IsValid)
				{
					ZQuery query = new ZQuery(OrgPartRelationSchema.OU_OP, L3_OP_ProductSKU);
					OrgPartRelation product = Factory.LoadTop1<OrgPartRelation>(query);
					L3_OH_Supplier = (product != null) ? product.OU_OH : ZGuid.Empty;
				}
				else
				{
					L3_OH_Supplier = ZGuid.Empty;
				}
			}
		}

		[List("Lookups.OSType")]
		public override ZString L3_OSType
		{
			get { return base.L3_OSType; }
			set
			{
				base.L3_OSType = value;
			}
		}

		[List("Lookups.LicenceType")]
		public override ZString L3_LicenceType
		{
			get { return base.L3_LicenceType; }
			set
			{
				base.L3_LicenceType = value;
			}
		}

		#endregion

		#region Part Description

		[MaxLength(OrgSupplierPart.Schema.OP_DescMaxLength)]
		public ZString PartDescription
		{
			get { return L3_OP_ProductSKU.IsEmpty || !L3_OP_ProductSKU.IsValid ? ZString.Empty : ProductSKU.OP_Desc; }
		}

		public ZPropertyInfo PartDescriptionInfo
		{
			get { return GetZPropertyInfo(Schema.PartDescription); }
		}

		#endregion

		#region IReadOnlySecurity Members

		protected bool GetReadOnlySecurity(PropertyDescriptor property)
		{
			return !EDISecurityCheckpoints.OrgLicenceModify3rdPartySoftware.IsAllowed || MetaData.GetReadOnlyExcludingMethodProvider(this, property);
		}

		#endregion
	}
}

