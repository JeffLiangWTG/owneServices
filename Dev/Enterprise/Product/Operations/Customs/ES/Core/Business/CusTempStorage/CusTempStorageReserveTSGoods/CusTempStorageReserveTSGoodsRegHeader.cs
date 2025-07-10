using System.Linq;
using CargoWise.Common;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.ZArchitecture.Schema;
using static Enterprise.Integration.Customs.TemporaryStorage;

namespace Enterprise.Customs.ES.Business.CusTempStorage;

public class CusTempStorageReserveTSGoodsRegHeader : NonPersistentBusinessObject
{
	public CusTempStorageReserveTSGoodsRegHeader(BusinessObjectFactory factory, ICusTempStorageRegHeader regHeader, CusTempStorageReserveTSGoodsDeclarationData declarationData) : base(factory)
	{
		TotalPackages = declarationData.TotalPackages;
		TotalGrossWeight = declarationData.TotalGrossWeight;
		ReserveTSGoodsCollection = [];
		GetRegLines(regHeader.PK, declarationData.GoodsItemNumber);
		SetInitialQuantities();
	}

	public static CusTempStorageReserveTSGoodsRegHeader New(BusinessObjectFactory factory, ICusTempStorageRegHeader regHeader, CusTempStorageReserveTSGoodsDeclarationData declarationData) => new(factory, regHeader, declarationData);

	void GetRegLines(ZGuid headerPK, ZInt goodsItemNumber)
	{
		var header = Factory.LoadTop1<EU.TemporaryStorage.Business.CusTempStorageRegHeader>(new ZQuery(CusTempStorageRegHeaderSchema.PK, headerPK));
		var lines = header.CusTempStorageRegLines.Where(l => l.SRL_CustomsStatus == "OPN" && l.RegLineItemPivots.OfType<EU.TemporaryStorage.Business.CusTempStorageRegLineItemPivot>().Any(p => p.RegLineItem.SRI_GoodsItemNumber == goodsItemNumber));
		lines.ForEach(l =>
		{
			var line = CusTempStorageReserveTSGoodsRegLine.New(l, TotalPackages, TotalGrossWeight);
			line.PropertyUpdated += OnLinePropertyUpdated;
			ReserveTSGoodsCollection.Add(line);
		});
	}

	void OnLinePropertyUpdated(object sender, CargoWise.Integration.PropertyUpdatedEventArgs e)
	{
		var lines = ReserveTSGoodsCollection.Cast<CusTempStorageReserveTSGoodsRegLine>();
		if (lines.IsCountMoreThan(1))
		{
			var regLine = sender as CusTempStorageReserveTSGoodsRegLine;
			var updatableLines = lines.Where(l => l.PK != regLine.PK);
			switch (e.PropertyName)
			{
				case CusTempStorageReserveTSGoodsRegLine.Schema.PackagesToUse:
					var diffPackages = TotalPackages != regLine.PackagesToUse ? TotalPackages - lines.Sum(l => l.PackagesToUse) : 0;
					if (diffPackages == 0)
					{
						updatableLines.ForEach(l =>
						{
							l.SuspendNotifications();
							l.PackagesToUse = 0;
							l.GrossWeightToUse = 0;
							l.ResumeNotifications();
						});

						regLine.SuspendNotifications();
						regLine.GrossWeightToUse = TotalGrossWeight;
						regLine.ResumeNotifications();
					}
					else
					{
						updatableLines.ForEach(l =>
						{
							l.SuspendNotifications();
							l.GrossWeightToUse = (TotalGrossWeight / TotalPackages) * l.PackagesToUse;
							l.ResumeNotifications();
						});

						var lineToUpdate = updatableLines.FirstOrDefault(l => l.HasErrors && l.NotificationsIncludingChildren.Any(e => e.Message.Contains(l.Validation.ExceedRemainingPackageQuantities))) ?? updatableLines.First();
						lineToUpdate.SuspendNotifications();
						lineToUpdate.PackagesToUse += diffPackages;
						lineToUpdate.GrossWeightToUse = (TotalGrossWeight / TotalPackages) * lineToUpdate.PackagesToUse;
						lineToUpdate.ResumeNotifications();

						regLine.SuspendNotifications();
						regLine.GrossWeightToUse = (TotalGrossWeight / TotalPackages) * regLine.PackagesToUse;
						regLine.ResumeNotifications();
					}
					break;
				case CusTempStorageReserveTSGoodsRegLine.Schema.GrossWeightToUse:
					var diffGrossWeight = TotalGrossWeight != regLine.GrossWeightToUse ? TotalGrossWeight - lines.Sum(l => l.GrossWeightToUse) : 0;
					if (diffGrossWeight == 0)
					{
						updatableLines.ForEach(l =>
						{
							l.SuspendNotifications();
							l.GrossWeightToUse = 0;
							l.ResumeNotifications();
						});
					}
					else
					{
						var lineToUpdate = updatableLines.FirstOrDefault(l => l.HasErrors && l.NotificationsIncludingChildren.Any(e => e.Message.Contains(l.Validation.ExceedRemainingGrossWeight))) ?? updatableLines.First();
						lineToUpdate.SuspendNotifications();
						lineToUpdate.GrossWeightToUse += diffGrossWeight;
						lineToUpdate.ResumeNotifications();
					}
					break;
			}
		}
	}

	void SetInitialQuantities()
	{
		var row = ReserveTSGoodsCollection.Cast<CusTempStorageReserveTSGoodsRegLine>().FirstOrDefault();
		if (row != null)
		{
			row.SuspendNotifications();
			row.PackagesToUse = TotalPackages;
			row.GrossWeightToUse = TotalGrossWeight;
			row.ResumeNotifications();
			IsPackageTypeBulk = row.IsPackageTypeBulk;
		}
	}

	public void RemoveAllReserveTSGoodsCollection() => ReserveTSGoodsCollection.RemoveAll();

	public readonly ZInt TotalPackages;
	public readonly ZDecimal TotalGrossWeight;
	public ZBool IsPackageTypeBulk { get; private set; }
	public CusTempStorageReserveTSGoodsRegLineCollection ReserveTSGoodsCollection { get; }
}
