namespace Enterprise.Customs.AU.Declaration.Business
{
	public class QuarantineCatchZoneValidation : Customs.Business.CusCodeDataValidation
	{
		public QuarantineCatchZoneValidation(QuarantineCatchZone parent)
			: base(parent)
		{
		}

		new QuarantineCatchZone Parent => (QuarantineCatchZone)base.Parent;

		protected override void CheckCY_Code()
		{
		}

		protected override void CheckCY_Data()
		{
			base.CheckCY_Data();
			var data = Parent.CY_Data;
			if (!data.IsEmpty)
			{
				var quarantineExDocHeader = (QuarantineExDocHeader)Parent.Parent;
				if (quarantineExDocHeader.QH_ProduceType != EXDOCCommodityCodes.Codes.Fish)
				{
					Parent.CY_DataInfo.AddMessageError("Origin catch zone may only be present when produce type is fish.");
				}
			}
		}
	}
}
