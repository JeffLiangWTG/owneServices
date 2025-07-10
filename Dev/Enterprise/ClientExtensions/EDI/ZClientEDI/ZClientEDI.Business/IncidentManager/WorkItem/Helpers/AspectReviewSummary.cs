using System;

namespace Enterprise.Client.EDI;

public readonly struct AspectReviewSummary(Guid aspectReviewPK, Guid aspectPK, string capability, string aspectName)
{
	public static AspectReviewSummary ForCapabilityAspect(Guid aspectReviewPK, Guid aspectPK, string capability, string aspectName)
		=> new(aspectReviewPK, aspectPK, capability, aspectName);

	public static AspectReviewSummary ForAssessAspect(Guid aspectReviewPK, Guid aspectPK, string aspectName)
		=> new(aspectReviewPK, aspectPK, null, aspectName);

	public string Capability { get; } = capability;
	public string AspectName { get; } = aspectName;
	public Guid PK { get; } = aspectReviewPK;
	public Guid AspectPK { get; } = aspectPK;

	public bool IsAssess => string.IsNullOrEmpty(Capability);

	public string GetTaskNotes()
		=> $"Please review \"{AspectName}\" Aspect Data: https://crikey.wtg.zone/AspectReview/{PK}";
}
