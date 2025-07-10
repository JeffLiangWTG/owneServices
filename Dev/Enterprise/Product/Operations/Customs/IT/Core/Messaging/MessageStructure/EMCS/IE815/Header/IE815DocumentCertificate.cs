using System;
using CargoWise.Common;
using CargoWise.Types;
using Enterprise.Customs.IT.Messaging.MessageFieldAttributes;

namespace Enterprise.Customs.IT.Messaging.EMCS;

public class IE815DocumentCertificate
{
	const int MaxIterationNumber = 9;
	public IE815DocumentCertificate(IDocumentCertificate documentCertificate, int numberIteration)
	{
		this.documentCertificate = Argument.NotNull(documentCertificate, "documentCertificate");
		var numberIterationName = nameof(numberIteration);
		NumberIteration = Argument.GreaterThanOrEqualToZero(numberIteration, numberIterationName);
		if (numberIteration > MaxIterationNumber)
		{
			throw new ArgumentOutOfRangeException(numberIterationName, FormattableString.Invariant($"{numberIterationName} must be less than {MaxIterationNumber}"));
		}
	}
	readonly IDocumentCertificate documentCertificate;

	[MessageLayout(Order = 0)]
	[MessageFieldStringRepresentation(CharType.Alphabetical, 1, true)]
	[MessageFieldRules("C", "C053")]
	public ZString IterationType => "L";

	[MessageLayout(Order = 1)]
	[MessageFieldIntegerRepresentation(3, true)]
	[MessageFieldRules("C", "C053")]
	public ZInt NumberIteration { get; }

	[MessageLayout(Order = 2)]
	[MessageFieldStringRepresentation(CharType.Alphanumeric, 350, false)]
	[MessageFieldRules("C", "C053", "D012")]
	public ZString DocumentDescription => documentCertificate.DocumentDescription;

	[MessageLayout(Order = 3)]
	[MessageFieldStringRepresentation(CharType.Alphabetical, 2, false)]
	[MessageFieldRules("C", "C053", "D012")]
	public ZString DocumentDescriptionLanguage => documentCertificate.DocumentDescriptionLanguage;

	[MessageLayout(Order = 4)]
	[MessageFieldStringRepresentation(CharType.Alphanumeric, 350, false)]
	[MessageFieldRules("C", "C053", "D012")]
	public ZString ReferenceOfDocument => documentCertificate.ReferenceOfDocument;

	[MessageLayout(Order = 5)]
	[MessageFieldStringRepresentation(CharType.Alphabetical, 2, false)]
	[MessageFieldRules("C", "C053", "C012")]
	public ZString ReferenceOfDocumentLanguage => documentCertificate.ReferenceOfDocumentLanguage;
}
