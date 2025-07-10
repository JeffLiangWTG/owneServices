namespace Enterprise.Customs.EU.Business.Documents.DocDataObjects;

public class CustomsEndorsementMaxLength
{
	public CustomsEndorsementMaxLength(int formLength, int numberLength, int customsLength, int issuingLength, int placeLength)
	{
		FormInDocument = formLength;
		NumberInDocument = numberLength;
		CustomsInDocument = customsLength;
		IssuingInDocument = issuingLength;
		PlaceInDocument = placeLength;
	}

	public int FormInDocument { get; set; }
	public int NumberInDocument { get; set; }
	public int CustomsInDocument { get; set; }
	public int IssuingInDocument { get; set; }
	public int PlaceInDocument { get; set; }
}
