using System;

namespace CargoWise.Main.Navigation;

#nullable disable
public record PublicHoliday
{
	public DateTime Date { get; set; }
	public string Weekday { get; set; }
	public string Month { get; set; }
	public string Description { get; set; }
	public string RegionCode { get; set; }
	public string RegionName { get; set; }
}
