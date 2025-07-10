using System.Data;
using CargoWise.EntityFramework;
using CargoWise.Types;
using CargoWiseOne.ResourceStrings;
using Enterprise.Customs.Business;

namespace Enterprise.Customs.JP.Common
{
	public class CusSeal : Business.CusSeal, IShortSequenceNumberLine
	{
		public CusSeal(BusinessObjectFactory factory, DataRow row) : base(factory, row)
		{
		}

		[ResourceStringData("Enterprise.Customs.JP.Common.CusSeal|BK_SealNumber", Caption = "Seal Number")]
		public override ZString BK_SealNumber { get => base.BK_SealNumber; set => base.BK_SealNumber = value; }

		public override ZString BK_ParentTableCode
		{
			get => base.BK_ParentTableCode;
			set
			{
				var oldValue = BK_ParentTableCode;
				base.BK_ParentTableCode = value;

				if (!IsCopying && oldValue != value)
				{
					SequenceNumberGenerator?.RecalculateWhenAdded(this);
				}
			}
		}

		public override void Delete()
		{
			if (!IsDeleted)
			{
				SequenceNumberGenerator?.RecalculateWhenAboutToBeDetachedOrDeleted(this);
			}
			base.Delete();
		}

		ZShort ISequenceNumberLine<ZShort>.SequenceNumber { get => BK_SequenceNumber; set => BK_SequenceNumber = value; }

		ZGuid ISequenceNumberLine.FKToHeader => BK_ParentID;

		ShortSequenceNumberGenerator SequenceNumberGenerator => GetSequenceNumberGeneratorCore();
		protected virtual ShortSequenceNumberGenerator GetSequenceNumberGeneratorCore() => null;
	}
}
