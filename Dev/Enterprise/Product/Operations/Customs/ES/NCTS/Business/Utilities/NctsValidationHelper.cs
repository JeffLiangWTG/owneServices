namespace Enterprise.Customs.ES.NCTS.Business
{
	public static class NctsValidationHelper
	{
		#region NctsDepartureHeaderContainer

		public static void CheckContainerHasBeenAssigned(this NctsDepartureHeaderContainer nctsDepartureHeaderContainer)
		{
			var departureGoodsItems = nctsDepartureHeaderContainer.Header.MovementHeader.GoodsItems;
			var hasBeenAssigned = false;
			var containerNum = nctsDepartureHeaderContainer.BC_ContainerNum;
			foreach (NctsDepartureCargoDesc goodsItem in departureGoodsItems)
			{
				if (goodsItem.ContainersSelected.Contains(containerNum))
				{
					hasBeenAssigned = true;
					break;
				}
			}
			if (!hasBeenAssigned)
			{
				nctsDepartureHeaderContainer.AddRowWarning(Res.GetString("D036D663-1368-4545-94F3-BDA5CFEF6DF6", "The container {0} is not assigned to any Good Item", containerNum));
			}
		}

		#endregion
	}
}
