using System;

namespace CargoWise.Main.Navigation;

#nullable disable
public class RecentMessage
{
	public Guid Id { get; set; }
	public string SenderName { get; set; }
	public string SenderCompanyName { get; set; }
	public Guid JobId { get; set; }
	public string JobTableCode { get; set; }
	public string JobType { get; set; }
	public string JobCode { get; set; }
	public string Body { get; set; }
	public DateTime PostedTime { get; set; }
	public string PostedTimeAgo { get; set; }
}
