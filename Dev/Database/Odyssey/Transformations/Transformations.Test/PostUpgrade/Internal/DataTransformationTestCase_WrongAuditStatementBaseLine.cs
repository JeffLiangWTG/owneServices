using System;
using System.Collections.Immutable;
using System.Diagnostics.CodeAnalysis;

namespace Enterprise.DbUpgrader.Transformation.DataModification.Testing;

partial class DataTransformationTestCase
{
	// DO NOT ADD ANY NEW TYPE HERE. THIS IS A BASELINE FOR TRANSFORMS THAT HAVE WRONG AUDIT STATEMENTS.
	// If you receive audit trigger errors, investigate your commands and provide correct audit values.
	[SuppressMessage("Style", "IDE0001")]
	static readonly ImmutableHashSet<Type> transformationsWithWrongAuditStatementsBaseLine = ImmutableHashSet.Create(new[]
	{
		typeof(Enterprise.DbUpgrader.Transformation.DataModification.Public.Customs.AddInfoTransformationBase.Testing.CopyAddInfoToOtherTableRealColumnDifferentColumnTypesTestClass),
		typeof(Enterprise.DbUpgrader.Transformation.DataModification.Public.Customs.AddInfoTransformationBase.Testing.CopyAddInfoToOtherTableRealColumnTestClass),
		typeof(Enterprise.DbUpgrader.Transformation.DataModification.Public.Customs.AddInfoTransformationBase.Testing.CopyAddInfoToRealColumnDifferentColumnTypesTestClass),
		typeof(Enterprise.DbUpgrader.Transformation.DataModification.Public.Customs.AddInfoTransformationBase.Testing.CopyAddInfoToRealColumnTestClass),
		typeof(Enterprise.DbUpgrader.Transformation.DataModification.Public.Security.DenyRootSecurityRightsByDefaultForGroups),
		typeof(Enterprise.DbUpgrader.Transformation.DataModification.Testing.EncryptValueInRegistryTransformForTest),
		typeof(Enterprise.DbUpgrader.Transformations.Transforms.BufferManagement.PopulateProcessHeaderTaskLowestOpenSequenceNumber),
		typeof(Enterprise.DbUpgrader.Transformations.BusinessIntelligence.RecreateIndexesForAuditDb),
		typeof(Enterprise.DbUpgrader.Transformations.PostUpgrade.Public.Glow.RemoveGlowUseDependencyGraphRegistryItem),
		typeof(Enterprise.DbUpgrader.Transformations.Transforms.ArchiveManager.ResetBiIsRequiredDeleteOrphanSubscriberRegistryItem),
		typeof(Enterprise.DbUpgrader.Transformations.Registry.ResetFountainVersionToDefault),
		typeof(Enterprise.DbUpgrader.Transformations.Transforms.ProductWarehouse.TransformStmALogToWhsInventoryHoldChangeLog),
		typeof(Enterprise.DbUpgrader.Transformations.Transforms.Customs.CA.UpdateAddInfoForPortOfExitAndPlaceOfReport),
		// We have added a new entry as part of defect fix WI00769394
		// The issue is that DocEngine transformations involve using BusinessObjectFactory to Load, Modify and Save BizOs
		// This code automatically will invoke SuspendAuditTriggers, which will make an assertion throw
		// Any approaches to avoid this factory save are hacks, and shouldn't be implemented to support this check
		typeof(Enterprise.DbUpgrader.Transformations.Transforms.Documents.UpdateScheduledReportTemplateForJobHistoryReportTransformation),
	});
}
