using System.Data;
using CargoWise.EntityFramework;
using CargoWise.Types;
using CargoWiseOne.ResourceStrings;
using Enterprise.Customs.Business;

namespace Enterprise.Customs.JP.Manifest.Business
{
	public class CusSeal : Common.CusSeal
	{
		public CusSeal(BusinessObjectFactory factory, DataRow row) : base(factory, row)
		{
		}

		AsycudaContainer parent;
		public AsycudaContainer Parent
		{
			get
			{
				if (parent == null && !BK_ParentTableCode.IsEmpty && !BK_ParentID.IsEmpty)
				{
					parent = Factory.Load<AsycudaContainer>(BK_ParentID);
				}
				return parent;
			}
		}

		[ResourceStringData("Enterprise.Customs.JP.Manifest.Business.CusSeal|BK_SealNumber", Caption = "Seal Number")]
		public override ZString BK_SealNumber
		{
			get => base.BK_SealNumber;
			set
			{
				if (BK_SealNumber != value)
				{
					base.BK_SealNumber = value;
					Parent?.MarkAsNeedingValidation();
				}
			}
		}

		protected override ShortSequenceNumberGenerator GetSequenceNumberGeneratorCore()
		{
			return Parent?.SealsSequenceNumberGenerator;
		}
	}
}
