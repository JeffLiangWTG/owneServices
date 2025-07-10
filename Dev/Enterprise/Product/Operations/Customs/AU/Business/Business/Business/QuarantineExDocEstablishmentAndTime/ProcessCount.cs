using CargoWise.Types;

namespace Enterprise.Customs.AU.Declaration.Business;

public class ProcessCount
{
	public ProcessCount(bool harvest, int catcherVessel, int catcherBoat, int freezing, bool packing, bool processing, bool slaughter, bool storage, int aquacultureFarm, ZDateTime packingStartDate, ZDateTime packingEndDate, ZDateTime freezingStartDate, ZDateTime slaughterEndDate)
	{
		Harvest = harvest;
		CatcherVessel = catcherVessel;
		CatcherBoat = catcherBoat;
		Freezing = freezing;
		Packing = packing;
		Processing = processing;
		Slaughter = slaughter;
		Storage = storage;
		PackingStartDate = packingStartDate;
		PackingEndDate = packingEndDate;
		FreezingStartDate = freezingStartDate;
		SlaughterEndDate = slaughterEndDate;
		AquacultureFarm = aquacultureFarm;
	}
	public readonly ZDateTime PackingStartDate;
	public readonly ZDateTime PackingEndDate;
	public readonly ZDateTime FreezingStartDate;
	public readonly ZDateTime SlaughterEndDate;
	public readonly bool Harvest;
	public readonly int CatcherVessel;
	public readonly int CatcherBoat;
	public readonly int Freezing;
	public readonly bool Packing;
	public readonly bool Processing;
	public readonly bool Slaughter;
	public readonly bool Storage;
	public readonly int AquacultureFarm;
}
