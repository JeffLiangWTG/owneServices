using System.Data;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.ZArchitecture.Core;
using ResString = ZClientEDI.Business.ResString;

namespace Enterprise.Client.EDI.MasterFiles.Business
{
	public class EDIGenericCharge : Accounting.Business.GenericCharge.GenericCharge
	{
		public EDIGenericCharge(BusinessObjectFactory factory, DataRow row) : base(factory, row)
		{
		}

		[TranslatableDataField(Schema.TableName, Schema.VC_Description, Schema.VC_DescriptionMaxLength, Schema.VC_Description, Type = typeof(EDIGenericCharge), Asmid = ResString.AssemblyId)]
		public override ZString VC_Description { get => base.VC_Description; set => base.VC_Description = value; }

		public MultilingualString VC_DescriptionMultilingual => GetMultilingual(VC_DescriptionInfo);
	}
}

