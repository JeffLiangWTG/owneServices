
namespace Enterprise.DataTransfer.Business
{
	public interface ITransferDataCollector
	{
		int UnitsProcessed { get; set; }
		bool BOF { get; set; }
		bool EOF { get; set; }
	}
}
