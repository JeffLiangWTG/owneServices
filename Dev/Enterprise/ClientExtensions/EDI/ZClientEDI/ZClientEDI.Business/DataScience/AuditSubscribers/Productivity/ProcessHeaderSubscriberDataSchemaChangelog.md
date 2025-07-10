# ProcessHeaderSubscriberDataSchema Changelog
(See [example](../ExampleChangelog.md) for how to update this document)

## [v11] - 2025-04-03
### Changed
- SpecificColumns
	- `FH_EffectiveNudge` (Made it SPARSE NULL)

## [v10] - 2025-01-10
### Changed
- SpecificColumns
	- `FH_CompletionStatement` (Changed data type from NVARCHAR(MAX) to NVARCHAR(512))

## [v9] - 2024-11-27
### Added
- SpecificColumns
	- `FH_CompletionStatement`

## [v8] - 2024-11-25
### Added
- SpecificColumns
	- `FH_BMT_BufferTimespan`

## [v7] - 2024-11-14
### Removed
- SpecificColumns
	- `FH_RequireReleaseGate`

### Changed
- SpecificColumns
	- `FH_FC_DedicatedBuffer` (Renamed from `FH_FC_NextComponent`)

## [v6] - 2024-10-24
### Added
- SpecificColumns
	- `FH_GB_EffectiveBranch`
	- `FH_GE_EffectiveDepartment`

## [v5] - 2024-08-23
### Added
- SpecificColumns
	- `FH_FC_NextComponent`
	- `FH_RequireReleaseGate`
	- `FH_GB_Branch`
	- `FH_GE_Department`
	- `FH_EffectiveAgreedDeliveryDateUtc`
	- `FH_ReleaseSequenceSortDateUtc`
	- `FH_LatestAcceptableReleaseDateUtc`

## [v4] - 2024-07-17
### Added
- SpecificColumns
	- `FH_DeadlineType`

## [v3] - 2024-06-24
### Added
- Columns
	- `FH_EffectiveNudge`

## [v2] - 2024-05-28
### Added
- Columns
	- `FH_IsApproved`

## [v1] - 2024-04-17

### Initial schema

- Columns
	- `FH_PK`
	- `FH_AgreedDeliveryDate`
	- `FH_AgreedDeliveryDateDefaultHoursOffset`
	- `FH_AgreedDeliveryDateDefaultsFrom`
	- `FH_AllowTaskAutoAssignment`
	- `FH_BufferPenetrationPercentWhenCompleted`
	- `FH_Category`
	- `FH_DateAcceptability`
	- `FH_DoNotStartBeforeDate`
	- `FH_EarliestStartDateDefaultsFrom`
	- `FH_EarliestStartDefaultHoursOffset`
	- `FH_FC_CurrentComponent`
	- `FH_FH_ParentHeader`
	- `FH_GG_ReleaseGroup`
	- `FH_IsActive`
	- `FH_IsCriticalHandover`
	- `FH_IsReleasableUnitParent`
	- `FH_IsReleaseGroupSetByTemplate`
	- `FH_IsStandby`
	- `FH_JobCode`
	- `FH_JobDescription`
	- `FH_LastTransferType`
	- `FH_P0_Template`
	- `FH_ParentId`
	- `FH_ParentTableCode`
	- `FH_ParentTemplateId`
	- `FH_PlannedDurationInMinutes`
	- `FH_ReleaseDateTime`
	- `FH_RemainingMinutesToComplete`
	- `FH_StaggeredReleaseDelayExpiry`
	- `FH_Status`
	- `FH_SystemCreateTimeUtc`
	- `FH_SystemCreateUser`
	- `FH_SystemLastEditTimeUtc`
	- `FH_SystemLastEditUser`
	- `FH_TaskLowestOpenSequenceNumber`
	- `FH_TaskPenetrationResetDateTimeUtc`
	- `FH_TimeDelayFactor`
	- `FH_TimeDelayMinutes`
	- `FH_VoteUpDownAmount`
	- `FH_WorkflowType`
